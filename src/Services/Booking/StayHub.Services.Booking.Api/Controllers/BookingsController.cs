using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Booking.Application.DTOs;
using StayHub.Services.Booking.Application.Features.CancelBooking;
using StayHub.Services.Booking.Application.Features.CheckInBooking;
using StayHub.Services.Booking.Application.Features.CompleteBooking;
using StayHub.Services.Booking.Application.Features.ConfirmBooking;
using StayHub.Services.Booking.Application.Features.CreateBooking;
using StayHub.Services.Booking.Application.Features.MarkNoShow;

namespace StayHub.Services.Booking.Api.Controllers;

/// <summary>
/// Booking management controller — reservation CRUD and lifecycle operations.
///
/// Authorization:
/// - POST /api/bookings → Authenticated (any logged-in user can create a booking)
/// - POST /api/bookings/{id}/confirm → HotelOwnerOrAdmin (confirm after payment)
/// - POST /api/bookings/{id}/check-in → HotelOwnerOrAdmin (hotel staff action)
/// - POST /api/bookings/{id}/complete → HotelOwnerOrAdmin (guest checkout)
/// - POST /api/bookings/{id}/cancel → Authenticated (guest cancels own booking)
/// - POST /api/bookings/{id}/no-show → HotelOwnerOrAdmin (guest did not arrive)
/// - GET /api/bookings/{id} → Authenticated (guest or hotel owner — added in commit 28)
///
/// UserId is always extracted from JWT claims, never from request body.
/// </summary>
[Route("api/bookings")]
public sealed class BookingsController : ApiController
{
    /// <summary>
    /// Create a new hotel room reservation.
    /// The guest's identity is extracted from the JWT token.
    ///
    /// Flow:
    /// 1. Validates the hotel and room via Hotel Service HTTP calls
    /// 2. Checks real-time availability
    /// 3. Creates a Pending booking with locked-in pricing
    /// 4. Returns the full booking DTO with confirmation number
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var guestUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new CreateBookingCommand(
            request.HotelId,
            request.RoomId,
            request.CheckIn,
            request.CheckOut,
            request.NumberOfGuests,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.SpecialRequests,
            guestUserId);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(result, nameof(GetById), new { id = result.IsSuccess ? result.Value.Id : (Guid?)null });
    }

    /// <summary>
    /// Confirm a booking after payment verification.
    /// Transition: Pending → Confirmed.
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ConfirmBookingCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Check in a guest.
    /// Transition: Confirmed → CheckedIn.
    /// </summary>
    [HttpPost("{id:guid}/check-in")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckIn(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new CheckInBookingCommand(id, userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Complete a booking (guest checks out).
    /// Transition: CheckedIn → Completed.
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new CompleteBookingCommand(id, userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancel a booking.
    /// Transition: Pending → Cancelled, Confirmed → Cancelled (requires reason).
    /// Only the guest who made the booking can cancel it.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelBookingRequest? request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new CancelBookingCommand(id, request?.CancellationReason, userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Mark a booking as no-show.
    /// Transition: Confirmed → NoShow.
    /// Hotel staff action when the guest does not arrive.
    /// </summary>
    [HttpPost("{id:guid}/no-show")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkNoShow(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new MarkNoShowCommand(id, userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a booking by ID — placeholder for commit 28.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        // Placeholder — will be implemented in commit 28 (guest booking queries)
        return NotFound();
    }

    /// <summary>
    /// Health check — confirms the Booking API is running.
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { service = "StayHub.Booking", status = "healthy" });
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────

/// <summary>
/// Request body for POST /api/bookings.
/// GuestUserId is NOT included — it comes from the JWT claim.
/// </summary>
public sealed record CreateBookingRequest(
    Guid HotelId,
    Guid RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int NumberOfGuests,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? SpecialRequests);

/// <summary>
/// Request body for POST /api/bookings/{id}/cancel.
/// Cancellation reason is optional for Pending, required for Confirmed bookings.
/// </summary>
public sealed record CancelBookingRequest(
    string? CancellationReason);
