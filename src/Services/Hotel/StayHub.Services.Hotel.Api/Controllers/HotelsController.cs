using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Application.Features.CreateHotel;
using StayHub.Services.Hotel.Application.Features.GetHotelById;
using StayHub.Services.Hotel.Application.Features.GetHotelsByOwner;
using StayHub.Services.Hotel.Application.Features.UpdateHotel;

namespace StayHub.Services.Hotel.Api.Controllers;

/// <summary>
/// Hotel management controller — CRUD operations for hotel listings.
///
/// Authorization:
/// - POST /api/hotels → HotelOwnerOrAdmin (create hotel listing)
/// - PUT /api/hotels/{id} → HotelOwnerOrAdmin (update own hotel; ownership verified in handler)
/// - GET /api/hotels/{id} → AllowAnonymous (public detail view)
/// - GET /api/hotels/my → HotelOwnerOrAdmin (list own hotels)
///
/// OwnerId is always extracted from JWT claims, never from request body.
/// The handler performs additional ownership verification for write operations.
/// </summary>
[Route("api/hotels")]
public sealed class HotelsController : ApiController
{
    /// <summary>
    /// Create a new hotel listing. Starts in Draft status.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateHotelRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new CreateHotelCommand(
            request.Name,
            request.Description,
            request.StarRating,
            request.Street,
            request.City,
            request.State,
            request.Country,
            request.ZipCode,
            request.Phone,
            request.Email,
            request.Website,
            request.CheckInTime,
            request.CheckOutTime,
            request.Latitude,
            request.Longitude,
            ownerId);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(result, nameof(GetById), new { id = result.IsSuccess ? result.Value.Id : (Guid?)null });
    }

    /// <summary>
    /// Update an existing hotel's information.
    /// Only the owner can update their hotel. Only Draft or Active hotels can be updated.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateHotelRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new UpdateHotelCommand(
            id,
            request.Name,
            request.Description,
            request.StarRating,
            request.Street,
            request.City,
            request.State,
            request.Country,
            request.ZipCode,
            request.Phone,
            request.Email,
            request.Website,
            request.CheckInTime,
            request.CheckOutTime,
            request.Latitude,
            request.Longitude,
            ownerId);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Get hotel details by ID. Public endpoint — no authentication required.
    /// Returns full hotel detail including rooms.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HotelDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetHotelByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Get all hotels owned by the authenticated user.
    /// Returns summary DTOs for the owner's dashboard.
    /// </summary>
    [HttpGet("my")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(typeof(IReadOnlyList<HotelSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyHotels(
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var query = new GetHotelsByOwnerQuery(ownerId);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }
}

// ── Request DTOs ────────────────────────────────────────────────────────
// Separate from command records so OwnerId is never in the request body.

/// <summary>
/// Request body for creating a hotel.
/// OwnerId excluded — set from JWT claims by the controller.
/// </summary>
public sealed record CreateHotelRequest(
    string Name,
    string Description,
    int StarRating,
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode,
    string Phone,
    string Email,
    string? Website,
    string? CheckInTime,
    string? CheckOutTime,
    double? Latitude,
    double? Longitude);

/// <summary>
/// Request body for updating a hotel.
/// OwnerId and HotelId excluded — set from JWT claims and route parameter by the controller.
/// </summary>
public sealed record UpdateHotelRequest(
    string Name,
    string Description,
    int StarRating,
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode,
    string Phone,
    string Email,
    string? Website,
    string CheckInTime,
    string CheckOutTime,
    double? Latitude,
    double? Longitude);
