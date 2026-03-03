using StayHub.Shared.CQRS;

namespace StayHub.Services.Booking.Application.Features.MarkNoShow;

/// <summary>
/// Command to mark a booking as no-show.
/// Transition: Confirmed → NoShow.
///
/// Called by hotel staff or an automated job when the guest
/// does not arrive by the check-in deadline.
/// UserId identifies who is performing the action (for audit).
/// </summary>
public sealed record MarkNoShowCommand(
    Guid BookingId,
    string UserId) : ICommand;
