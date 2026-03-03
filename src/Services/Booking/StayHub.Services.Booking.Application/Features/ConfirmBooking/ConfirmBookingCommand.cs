using StayHub.Shared.CQRS;

namespace StayHub.Services.Booking.Application.Features.ConfirmBooking;

/// <summary>
/// Command to confirm a booking after successful payment.
/// Transition: Pending → Confirmed.
///
/// Typically called by the Payment Service (or admin) after payment is verified.
/// The handler validates ownership — only the hotel owner or admin should confirm.
/// </summary>
public sealed record ConfirmBookingCommand(
    Guid BookingId) : ICommand;
