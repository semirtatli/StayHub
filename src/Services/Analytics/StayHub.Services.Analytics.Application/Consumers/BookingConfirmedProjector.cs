using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Services.Analytics.Application.Features.RecordAnalyticsEvent;
using StayHub.Services.Analytics.Application.IntegrationEvents;
using StayHub.Services.Analytics.Domain.Enums;

namespace StayHub.Services.Analytics.Application.Consumers;

/// <summary>
/// Projects BookingConfirmed events into revenue, occupancy, and performance read models.
/// </summary>
public sealed class BookingConfirmedProjector : INotificationHandler<BookingConfirmedIntegrationEvent>
{
    private readonly ISender _mediator;
    private readonly ILogger<BookingConfirmedProjector> _logger;

    public BookingConfirmedProjector(ISender mediator, ILogger<BookingConfirmedProjector> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(
        BookingConfirmedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Projecting BookingConfirmed: BookingId={BookingId}, HotelId={HotelId}, Amount={Amount}",
            notification.BookingId, notification.HotelId, notification.TotalAmount);

        await _mediator.Send(new RecordAnalyticsEventCommand(
            EventType: AnalyticsEventType.BookingConfirmed,
            HotelId: notification.HotelId,
            BookingId: notification.BookingId,
            UserId: notification.UserId,
            HotelName: notification.HotelName,
            Amount: notification.TotalAmount,
            CheckInDate: notification.CheckInDate,
            CheckOutDate: notification.CheckOutDate,
            RoomCount: notification.RoomCount,
            TotalRooms: notification.TotalRooms), cancellationToken);
    }
}
