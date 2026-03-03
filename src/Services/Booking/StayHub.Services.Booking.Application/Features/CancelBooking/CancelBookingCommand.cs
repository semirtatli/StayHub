using StayHub.Shared.CQRS;

namespace StayHub.Services.Booking.Application.Features.CancelBooking;

/// <summary>
/// Command to cancel a booking.
/// Transition: Pending → Cancelled, Confirmed → Cancelled (requires reason).
///
/// Can be initiated by the guest (their own booking) or admin.
/// The handler verifies the calling user is the booking owner.
/// </summary>
public sealed record CancelBookingCommand(
    Guid BookingId,
    string? CancellationReason,
    string UserId) : ICommand;
