using Microsoft.Extensions.Logging;
using StayHub.Services.Booking.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Booking.Application.Features.CancelBooking;

/// <summary>
/// Handles booking cancellation — Pending → Cancelled or Confirmed → Cancelled.
///
/// Authorization:
/// - Guest can cancel their own bookings.
/// - Cancellation of a Confirmed booking requires a reason (enforced by domain).
///
/// Raises BookingStatusChangedEvent + BookingCancelledEvent for downstream consumers:
/// - Release room availability in Hotel Service
/// - Trigger refund in Payment Service (for Confirmed bookings)
/// - Send cancellation notification
///
/// TransactionBehavior commits the unit of work after a successful result.
/// </summary>
public sealed class CancelBookingCommandHandler : ICommandHandler<CancelBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<CancelBookingCommandHandler> _logger;

    public CancelBookingCommandHandler(
        IBookingRepository bookingRepository,
        ILogger<CancelBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        CancelBookingCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(
            request.BookingId, cancellationToken);

        if (booking is null)
            return Result.Failure(BookingErrors.Booking.NotFound);

        // Only the guest who made the booking can cancel it
        if (booking.GuestUserId != request.UserId)
            return Result.Failure(BookingErrors.Booking.NotGuest);

        try
        {
            booking.Cancel(request.CancellationReason);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(BookingErrors.Booking.InvalidStatusTransition);
        }
        catch (ArgumentException)
        {
            // Domain requires cancellation reason for Confirmed bookings
            return Result.Failure(BookingErrors.Booking.CancellationReasonRequired);
        }

        _bookingRepository.Update(booking);

        _logger.LogInformation(
            "Booking {BookingId} cancelled by user {UserId}. Reason: {Reason}",
            booking.Id, request.UserId, request.CancellationReason ?? "(none)");

        return Result.Success();
    }
}
