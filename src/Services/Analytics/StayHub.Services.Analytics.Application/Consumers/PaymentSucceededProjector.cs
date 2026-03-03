using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Services.Analytics.Application.Features.RecordAnalyticsEvent;
using StayHub.Services.Analytics.Application.IntegrationEvents;
using StayHub.Services.Analytics.Domain.Enums;

namespace StayHub.Services.Analytics.Application.Consumers;

/// <summary>
/// Projects PaymentSucceeded events — records in the analytics event log.
/// Revenue is already tracked via BookingConfirmed; this provides audit trail.
/// </summary>
public sealed class PaymentSucceededProjector : INotificationHandler<PaymentSucceededIntegrationEvent>
{
    private readonly ISender _mediator;
    private readonly ILogger<PaymentSucceededProjector> _logger;

    public PaymentSucceededProjector(ISender mediator, ILogger<PaymentSucceededProjector> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(
        PaymentSucceededIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Projecting PaymentSucceeded: PaymentId={PaymentId}, HotelId={HotelId}, Amount={Amount}",
            notification.PaymentId, notification.HotelId, notification.Amount);

        await _mediator.Send(new RecordAnalyticsEventCommand(
            EventType: AnalyticsEventType.PaymentReceived,
            HotelId: notification.HotelId,
            BookingId: notification.BookingId,
            Amount: notification.Amount), cancellationToken);
    }
}
