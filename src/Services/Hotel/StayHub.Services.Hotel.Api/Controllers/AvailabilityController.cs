using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Hotel.Application.Features.BlockDates;
using StayHub.Services.Hotel.Application.Features.CheckAvailability;
using StayHub.Services.Hotel.Application.Features.SetRoomAvailability;
using StayHub.Services.Hotel.Application.Features.UnblockDates;

namespace StayHub.Services.Hotel.Api.Controllers;

/// <summary>
/// Room availability management — the core inventory engine of the Hotel Service.
///
/// Endpoints:
/// - GET  /api/hotels/{hotelId}/availability?checkIn&amp;checkOut  → public availability check
/// - PUT  /api/hotels/{hotelId}/rooms/{roomId}/availability       → set inventory for date range (owner)
/// - POST /api/hotels/{hotelId}/rooms/{roomId}/availability/block → block dates (owner)
/// - POST /api/hotels/{hotelId}/rooms/{roomId}/availability/unblock → unblock dates (owner)
/// </summary>
[Route("api/hotels/{hotelId:guid}")]
public sealed class AvailabilityController : ApiController
{
    /// <summary>
    /// Check room availability for a hotel within a date range.
    /// Returns per-room-type availability with pricing and blocked-date info.
    /// Public endpoint — no authentication required.
    /// </summary>
    [HttpGet("availability")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HotelAvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckAvailability(
        Guid hotelId,
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut,
        CancellationToken cancellationToken)
    {
        var query = new CheckAvailabilityQuery(hotelId, checkIn, checkOut);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Set room availability (inventory + optional price override) for a date range.
    /// Creates records for uninitialized dates, updates existing ones.
    /// Only the hotel owner can manage availability.
    /// </summary>
    [HttpPut("rooms/{roomId:guid}/availability")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAvailability(
        Guid hotelId,
        Guid roomId,
        [FromBody] SetAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new SetRoomAvailabilityCommand(
            hotelId,
            roomId,
            request.FromDate,
            request.ToDate,
            request.TotalInventory,
            request.PriceOverride,
            ownerId);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Block dates for a room (maintenance, renovation, seasonal closure).
    /// Blocked dates cannot be booked regardless of available inventory.
    /// </summary>
    [HttpPost("rooms/{roomId:guid}/availability/block")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BlockDates(
        Guid hotelId,
        Guid roomId,
        [FromBody] BlockDatesRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new BlockDatesCommand(
            hotelId,
            roomId,
            request.FromDate,
            request.ToDate,
            request.Reason,
            ownerId);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Unblock previously blocked dates for a room.
    /// Restores dates to bookable status.
    /// </summary>
    [HttpPost("rooms/{roomId:guid}/availability/unblock")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnblockDates(
        Guid hotelId,
        Guid roomId,
        [FromBody] UnblockDatesRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new UnblockDatesCommand(
            hotelId,
            roomId,
            request.FromDate,
            request.ToDate,
            ownerId);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}

// ── Request DTOs ────────────────────────────────────────────────────────

/// <summary>
/// Request body for setting room availability.
/// </summary>
public sealed record SetAvailabilityRequest(
    DateOnly FromDate,
    DateOnly ToDate,
    int TotalInventory,
    decimal? PriceOverride);

/// <summary>
/// Request body for blocking dates.
/// </summary>
public sealed record BlockDatesRequest(
    DateOnly FromDate,
    DateOnly ToDate,
    string? Reason);

/// <summary>
/// Request body for unblocking dates.
/// </summary>
public sealed record UnblockDatesRequest(
    DateOnly FromDate,
    DateOnly ToDate);
