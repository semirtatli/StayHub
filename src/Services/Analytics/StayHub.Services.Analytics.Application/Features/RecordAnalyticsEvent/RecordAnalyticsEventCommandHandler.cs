using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Services.Analytics.Domain.Entities;
using StayHub.Services.Analytics.Domain.Enums;
using StayHub.Services.Analytics.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Analytics.Application.Features.RecordAnalyticsEvent;

/// <summary>
/// Records a raw analytics event and incrementally updates the relevant
/// read-model projections (revenue, occupancy, hotel performance).
/// TransactionBehavior commits all changes atomically.
/// </summary>
public sealed class RecordAnalyticsEventCommandHandler : ICommandHandler<RecordAnalyticsEventCommand>
{
    private readonly IAnalyticsRepository _repository;
    private readonly ILogger<RecordAnalyticsEventCommandHandler> _logger;

    public RecordAnalyticsEventCommandHandler(
        IAnalyticsRepository repository,
        ILogger<RecordAnalyticsEventCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        RecordAnalyticsEventCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Append raw analytics event
        var analyticsEvent = AnalyticsEvent.Create(
            request.HotelId,
            request.BookingId,
            request.UserId,
            request.EventType,
            request.Amount,
            metadata: null,
            DateTime.UtcNow);

        _repository.AddEvent(analyticsEvent);

        // 2. Update projections based on event type
        switch (request.EventType)
        {
            case AnalyticsEventType.BookingConfirmed:
                await HandleBookingConfirmed(request, cancellationToken);
                break;

            case AnalyticsEventType.BookingCancelled:
                await HandleBookingCancelled(request, cancellationToken);
                break;

            case AnalyticsEventType.RefundProcessed:
                await HandleRefundProcessed(request, cancellationToken);
                break;

            case AnalyticsEventType.ReviewSubmitted:
                await HandleReviewSubmitted(request, cancellationToken);
                break;

            case AnalyticsEventType.PaymentReceived:
                // Revenue is already tracked via BookingConfirmed projection.
                // PaymentReceived events are stored in the raw event log for audit.
                _logger.LogInformation(
                    "Payment received event recorded for Hotel={HotelId}, Booking={BookingId}, Amount={Amount}",
                    request.HotelId, request.BookingId, request.Amount);
                break;
        }

        return Result.Success();
    }

    private async Task HandleBookingConfirmed(
        RecordAnalyticsEventCommand request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Update revenue snapshot
        var revenueSnapshot = await _repository.GetRevenueSnapshotAsync(
            request.HotelId, today, cancellationToken);

        if (revenueSnapshot is null)
        {
            revenueSnapshot = DailyRevenueSnapshot.Create(request.HotelId, today);
            _repository.AddRevenueSnapshot(revenueSnapshot);
        }

        revenueSnapshot.AddBooking(request.Amount);

        // Update occupancy snapshot (for each day of the stay)
        if (request.CheckInDate.HasValue && request.CheckOutDate.HasValue)
        {
            for (var date = request.CheckInDate.Value;
                 date < request.CheckOutDate.Value;
                 date = date.AddDays(1))
            {
                var occupancy = await _repository.GetOccupancySnapshotAsync(
                    request.HotelId, date, cancellationToken);

                if (occupancy is null)
                {
                    occupancy = OccupancySnapshot.Create(
                        request.HotelId, date, request.TotalRooms);
                    _repository.AddOccupancySnapshot(occupancy);
                }

                occupancy.RecordBooking(request.RoomCount);
            }
        }

        // Update hotel performance summary
        await GetOrCreatePerformance(request.HotelId, request.HotelName,
            summary => summary.RecordBooking(request.Amount), cancellationToken);

        _logger.LogInformation(
            "Projected BookingConfirmed: Hotel={HotelId}, Amount={Amount}",
            request.HotelId, request.Amount);
    }

    private async Task HandleBookingCancelled(
        RecordAnalyticsEventCommand request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Update revenue snapshot
        var revenueSnapshot = await _repository.GetRevenueSnapshotAsync(
            request.HotelId, today, cancellationToken);

        if (revenueSnapshot is null)
        {
            revenueSnapshot = DailyRevenueSnapshot.Create(request.HotelId, today);
            _repository.AddRevenueSnapshot(revenueSnapshot);
        }

        revenueSnapshot.AddCancellation();

        // Release occupancy for cancelled booking dates
        if (request.CheckInDate.HasValue && request.CheckOutDate.HasValue)
        {
            for (var date = request.CheckInDate.Value;
                 date < request.CheckOutDate.Value;
                 date = date.AddDays(1))
            {
                var occupancy = await _repository.GetOccupancySnapshotAsync(
                    request.HotelId, date, cancellationToken);

                occupancy?.CancelBooking(request.RoomCount);
            }
        }

        // Update hotel performance
        await GetOrCreatePerformance(request.HotelId, request.HotelName,
            summary => summary.RecordCancellation(), cancellationToken);

        _logger.LogInformation(
            "Projected BookingCancelled: Hotel={HotelId}",
            request.HotelId);
    }

    private async Task HandleRefundProcessed(
        RecordAnalyticsEventCommand request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var revenueSnapshot = await _repository.GetRevenueSnapshotAsync(
            request.HotelId, today, cancellationToken);

        if (revenueSnapshot is null)
        {
            revenueSnapshot = DailyRevenueSnapshot.Create(request.HotelId, today);
            _repository.AddRevenueSnapshot(revenueSnapshot);
        }

        revenueSnapshot.AddRefund(request.Amount);

        // Update hotel performance
        await GetOrCreatePerformance(request.HotelId, request.HotelName,
            summary => summary.RecordRefund(request.Amount), cancellationToken);

        _logger.LogInformation(
            "Projected RefundProcessed: Hotel={HotelId}, Amount={Amount}",
            request.HotelId, request.Amount);
    }

    private async Task HandleReviewSubmitted(
        RecordAnalyticsEventCommand request,
        CancellationToken cancellationToken)
    {
        await GetOrCreatePerformance(request.HotelId, request.HotelName,
            summary => summary.RecordReview(request.Rating), cancellationToken);

        _logger.LogInformation(
            "Projected ReviewSubmitted: Hotel={HotelId}, Rating={Rating}",
            request.HotelId, request.Rating);
    }

    private async Task GetOrCreatePerformance(
        Guid hotelId,
        string? hotelName,
        Action<HotelPerformanceSummary> updateAction,
        CancellationToken cancellationToken)
    {
        var summary = await _repository.GetHotelPerformanceAsync(hotelId, cancellationToken);

        if (summary is null)
        {
            summary = HotelPerformanceSummary.Create(hotelId, hotelName ?? "Unknown");
            _repository.AddHotelPerformance(summary);
        }

        updateAction(summary);
    }
}
