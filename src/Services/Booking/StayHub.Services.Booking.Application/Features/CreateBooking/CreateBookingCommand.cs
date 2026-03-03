using StayHub.Services.Booking.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Booking.Application.Features.CreateBooking;

/// <summary>
/// Command to create a new hotel room reservation.
///
/// Flow: Controller → MediatR pipeline (Validation → Logging → Transaction → Handler)
///       → HTTP calls to Hotel Service for availability validation
///       → creates BookingEntity aggregate → persisted via TransactionBehavior.
///
/// GuestUserId is set by the controller from the authenticated user's JWT claims,
/// not from the request body — prevents users from creating bookings for other accounts.
/// </summary>
public sealed record CreateBookingCommand(
    Guid HotelId,
    Guid RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int NumberOfGuests,

    // Guest info snapshot
    string FirstName,
    string LastName,
    string Email,
    string? Phone,

    // Optional
    string? SpecialRequests,

    // Set by controller from JWT, not from request body
    string GuestUserId) : ICommand<BookingDto>;
