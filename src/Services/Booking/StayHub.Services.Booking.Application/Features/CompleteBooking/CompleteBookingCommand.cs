using StayHub.Shared.CQRS;

namespace StayHub.Services.Booking.Application.Features.CompleteBooking;

/// <summary>
/// Command to complete a booking (guest checks out).
/// Transition: CheckedIn → Completed.
///
/// Called by hotel staff when the guest departs.
/// UserId identifies who is performing the action (for audit).
/// </summary>
public sealed record CompleteBookingCommand(
    Guid BookingId,
    string UserId) : ICommand;
