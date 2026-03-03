using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Services.Analytics.Application.Features.RecordAnalyticsEvent;
using StayHub.Services.Analytics.Application.IntegrationEvents;
using StayHub.Services.Analytics.Domain.Enums;

namespace StayHub.Services.Analytics.Application.Consumers;

/// <summary>
/// Projects BookingCancelled events — updates cancellation counts and releases occupancy.
/// </summary>
public sealed class BookingCancelledProjector : INotificationHandler<BookingCancelledIntegrationEvent>
{
    private readonly ISender _mediator;
    private readonly ILogger<BookingCancelledProjector> _logger;

    public BookingCancelledProjector(ISender mediator, ILogger<BookingCancelledProjector> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(
        BookingCancelledIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Projecting BookingCancelled: BookingId={BookingId}, HotelId={HotelId}",
            notification.BookingId, notification.HotelId);

        await _mediator.Send(new RecordAnalyticsEventCommand(
            EventType: AnalyticsEventType.BookingCancelled,
            HotelId: notification.HotelId,
            BookingId: notification.BookingId,
            Amount: notification.Amount,
            RoomCount: notification.RoomCount), cancellationToken);
    }
}
