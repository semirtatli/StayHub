using StayHub.Shared.CQRS;

namespace StayHub.Services.Booking.Application.Features.CheckInBooking;

/// <summary>
/// Command to check in a guest.
/// Transition: Confirmed → CheckedIn.
///
/// Called by hotel staff or via self-service check-in.
/// UserId identifies who is performing the action (for audit).
/// </summary>
public sealed record CheckInBookingCommand(
    Guid BookingId,
    string UserId) : ICommand;
