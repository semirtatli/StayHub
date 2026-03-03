using System.Globalization;
using Microsoft.Extensions.Logging;
using StayHub.Services.Booking.Application.Abstractions;
using StayHub.Services.Booking.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Booking.Application.Features.CancelBooking;

/// <summary>
/// Handles booking cancellation — Pending → Cancelled or Confirmed → Cancelled.
///
/// Steps:
/// 1. Fetch booking, verify existence and guest ownership
/// 2. For Confirmed bookings: fetch the hotel's cancellation policy via HTTP
/// 3. Calculate refund percentage based on days until check-in
/// 4. Cancel the booking with the calculated refund percentage
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
    private readonly IHotelServiceClient _hotelServiceClient;
    private readonly ILogger<CancelBookingCommandHandler> _logger;

    public CancelBookingCommandHandler(
        IBookingRepository bookingRepository,
        IHotelServiceClient hotelServiceClient,
        ILogger<CancelBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _hotelServiceClient = hotelServiceClient;
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

        // Calculate refund percentage from cancellation policy
        int? refundPercentage = null;

        if (booking.Status == Domain.Enums.BookingStatus.Confirmed)
        {
            var policy = await _hotelServiceClient.GetCancellationPolicyAsync(
                booking.HotelId, cancellationToken);

            if (policy is not null)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var daysBeforeCheckIn = booking.StayPeriod.CheckIn.DayNumber - today.DayNumber;
                refundPercentage = CalculateRefundPercentage(policy, Math.Max(0, daysBeforeCheckIn));

                _logger.LogInformation(
                    "Cancellation policy for hotel {HotelId}: {PolicyType}, {DaysBeforeCheckIn} days before check-in → {RefundPct}% refund",
                    booking.HotelId, policy.PolicyType, daysBeforeCheckIn, refundPercentage);
            }
            else
            {
                // Default to 100% refund if policy can't be fetched (fail-safe for guest)
                refundPercentage = 100;
                _logger.LogWarning(
                    "Could not fetch cancellation policy for hotel {HotelId}, defaulting to 100% refund",
                    booking.HotelId);
            }
        }

        try
        {
            booking.Cancel(request.CancellationReason, refundPercentage);
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
            "Booking {BookingId} cancelled by user {UserId}. Reason: {Reason}, Refund: {RefundPct}%",
            booking.Id, request.UserId, request.CancellationReason ?? "(none)",
            refundPercentage?.ToString(CultureInfo.InvariantCulture) ?? "N/A");

        return Result.Success();
    }

    /// <summary>
    /// Calculate the refund percentage based on the cancellation policy rules.
    /// Mirrors the logic in Hotel.Domain.ValueObjects.CancellationPolicy.CalculateRefundPercentage.
    /// </summary>
    private static int CalculateRefundPercentage(CancellationPolicyResponse policy, int daysBeforeCheckIn)
    {
        if (string.Equals(policy.PolicyType, "NonRefundable", StringComparison.OrdinalIgnoreCase))
            return 0;

        if (daysBeforeCheckIn >= policy.FreeCancellationDays)
            return 100;

        if (daysBeforeCheckIn >= policy.PartialRefundDays)
            return policy.PartialRefundPercentage;

        return 0;
    }
}
