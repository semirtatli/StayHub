using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Hotel.Application.Abstractions;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Application.Features.DeleteHotelPhoto;
using StayHub.Services.Hotel.Application.Features.ReorderHotelPhotos;
using StayHub.Services.Hotel.Application.Features.UploadHotelPhoto;

namespace StayHub.Services.Hotel.Api.Controllers;

/// <summary>
/// Photo management controller for hotels.
///
/// Upload flow:
/// 1. Client sends multipart/form-data with file
/// 2. Controller uploads via IFileStorageService → gets URL
/// 3. Controller creates command with URL → handler adds to domain
/// 4. If command fails, controller deletes the orphaned file
///
/// Authorization:
/// - POST   /api/hotels/{hotelId}/photos          → HotelOwnerOrAdmin (upload)
/// - DELETE /api/hotels/{hotelId}/photos           → HotelOwnerOrAdmin (delete)
/// - PUT    /api/hotels/{hotelId}/photos/reorder   → HotelOwnerOrAdmin (reorder)
///
/// Room photos are managed through the Room CRUD endpoints (AddRoom/UpdateRoom).
/// </summary>
[Route("api/hotels/{hotelId:guid}/photos")]
[Authorize(Policy = "HotelOwnerOrAdmin")]
public sealed class HotelPhotosController : ApiController
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private readonly IFileStorageService _fileStorageService;

    public HotelPhotosController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Upload a photo for a hotel. The file is saved to storage and the URL
    /// is added to the hotel's photo gallery. Optionally set as cover image.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<IActionResult> Upload(
        Guid hotelId,
        IFormFile file,
        [FromQuery] bool setAsCover = false,
        CancellationToken cancellationToken = default)
    {
        // Validate file
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "File is required." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { error = $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}" });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { error = $"File size exceeds the limit of {MaxFileSizeBytes / (1024 * 1024)} MB." });
        }

        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Upload file to storage
        string photoUrl;
        await using (var stream = file.OpenReadStream())
        {
            photoUrl = await _fileStorageService.UploadAsync(
                stream,
                file.FileName,
                $"hotels/{hotelId}",
                cancellationToken);
        }

        // Create command to add URL to domain
        var command = new UploadHotelPhotoCommand(hotelId, photoUrl, setAsCover, ownerId);
        var result = await Mediator.Send(command, cancellationToken);

        // If command failed, clean up the uploaded file
        if (result.IsFailure)
        {
            await _fileStorageService.DeleteAsync(photoUrl, cancellationToken);
            return HandleResult(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a photo from a hotel's gallery and remove the file from storage.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid hotelId,
        [FromBody] DeletePhotoRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new DeleteHotelPhotoCommand(hotelId, request.PhotoUrl, ownerId);
        var result = await Mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleResult(result);
        }

        // Delete the physical file after successful domain operation
        await _fileStorageService.DeleteAsync(request.PhotoUrl, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Reorder the hotel's photo gallery. The request must contain exactly
    /// the same photo URLs as the current gallery in the desired order.
    /// </summary>
    [HttpPut("reorder")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reorder(
        Guid hotelId,
        [FromBody] ReorderPhotosRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new ReorderHotelPhotosCommand(hotelId, request.PhotoUrls, ownerId);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}

// ── Request DTOs ────────────────────────────────────────────────────────

/// <summary>
/// Request body for deleting a hotel photo.
/// </summary>
public sealed record DeletePhotoRequest(string PhotoUrl);

/// <summary>
/// Request body for reordering hotel photos.
/// </summary>
public sealed record ReorderPhotosRequest(IReadOnlyList<string> PhotoUrls);
