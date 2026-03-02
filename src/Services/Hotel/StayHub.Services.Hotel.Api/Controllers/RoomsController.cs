using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Application.Features.AddRoom;
using StayHub.Services.Hotel.Application.Features.GetRoomsByHotel;
using StayHub.Services.Hotel.Application.Features.RemoveRoom;
using StayHub.Services.Hotel.Application.Features.UpdateRoom;

namespace StayHub.Services.Hotel.Api.Controllers;

/// <summary>
/// Room management controller — nested under hotels.
///
/// Authorization:
/// - POST   /api/hotels/{hotelId}/rooms         → HotelOwnerOrAdmin (add room)
/// - PUT    /api/hotels/{hotelId}/rooms/{roomId} → HotelOwnerOrAdmin (update room)
/// - DELETE /api/hotels/{hotelId}/rooms/{roomId} → HotelOwnerOrAdmin (remove room)
/// - GET    /api/hotels/{hotelId}/rooms          → AllowAnonymous    (list rooms)
///
/// Rooms are always accessed in the context of their parent hotel.
/// Ownership is verified by the command handlers via the Hotel aggregate.
/// </summary>
[Route("api/hotels/{hotelId:guid}/rooms")]
public sealed class RoomsController : ApiController
{
    /// <summary>
    /// Add a room to a hotel.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddRoom(
        Guid hotelId,
        [FromBody] AddRoomRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new AddRoomCommand(
            hotelId,
            request.Name,
            request.Description,
            request.RoomType,
            request.MaxOccupancy,
            request.BasePrice,
            request.Currency,
            request.TotalInventory,
            request.SizeInSquareMeters,
            request.BedConfiguration,
            request.Amenities,
            request.PhotoUrls,
            ownerId);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(
            result,
            nameof(GetRooms),
            new { hotelId });
    }

    /// <summary>
    /// Update a room in a hotel.
    /// </summary>
    [HttpPut("{roomId:guid}")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateRoom(
        Guid hotelId,
        Guid roomId,
        [FromBody] UpdateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new UpdateRoomCommand(
            hotelId,
            roomId,
            request.Name,
            request.Description,
            request.RoomType,
            request.MaxOccupancy,
            request.BasePrice,
            request.Currency,
            request.TotalInventory,
            request.SizeInSquareMeters,
            request.BedConfiguration,
            request.Amenities,
            request.PhotoUrls,
            ownerId);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Remove a room from a hotel.
    /// </summary>
    [HttpDelete("{roomId:guid}")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRoom(
        Guid hotelId,
        Guid roomId,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new RemoveRoomCommand(hotelId, roomId, ownerId);
        var result = await Mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleResult(result);
        }

        return NoContent();
    }

    /// <summary>
    /// Get all rooms for a hotel. Public endpoint.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<RoomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRooms(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        var query = new GetRoomsByHotelQuery(hotelId);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }
}

// ── Request DTOs ────────────────────────────────────────────────────────

/// <summary>
/// Request body for adding a room. OwnerId excluded — set from JWT claims.
/// </summary>
public sealed record AddRoomRequest(
    string Name,
    string Description,
    string RoomType,
    int MaxOccupancy,
    decimal BasePrice,
    string Currency,
    int TotalInventory,
    decimal? SizeInSquareMeters,
    string? BedConfiguration,
    IReadOnlyList<string>? Amenities,
    IReadOnlyList<string>? PhotoUrls);

/// <summary>
/// Request body for updating a room. OwnerId and IDs excluded — set from JWT/route.
/// </summary>
public sealed record UpdateRoomRequest(
    string Name,
    string Description,
    string RoomType,
    int MaxOccupancy,
    decimal BasePrice,
    string Currency,
    int TotalInventory,
    decimal? SizeInSquareMeters,
    string? BedConfiguration,
    IReadOnlyList<string>? Amenities,
    IReadOnlyList<string>? PhotoUrls);
