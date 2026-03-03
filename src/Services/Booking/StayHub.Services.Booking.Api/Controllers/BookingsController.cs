using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Booking.Application.DTOs;
using StayHub.Services.Booking.Application.Features.CreateBooking;

namespace StayHub.Services.Booking.Api.Controllers;

/// <summary>
/// Booking management controller — reservation CRUD operations.
///
/// Authorization:
/// - POST /api/bookings → Authenticated (any logged-in user can create a booking)
/// - GET /api/bookings/{id} → Authenticated (guest or hotel owner — added in commit 28)
///
/// GuestUserId is always extracted from JWT claims, never from request body.
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
