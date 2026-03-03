using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Services.Notification.Application.Features.SendNotification;
using StayHub.Services.Notification.Application.IntegrationEvents;
using StayHub.Services.Notification.Domain.Enums;

namespace StayHub.Services.Notification.Application.Consumers;

/// <summary>
/// Consumes PaymentFailedIntegrationEvent and sends a failure notification email.
/// </summary>
public sealed class PaymentFailedConsumer : INotificationHandler<PaymentFailedIntegrationEvent>
{
    private readonly ISender _mediator;
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(ISender mediator, ILogger<PaymentFailedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(PaymentFailedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing payment failure for PaymentId={PaymentId}, BookingId={BookingId}",
            notification.PaymentId, notification.BookingId);

        var templateData = new Dictionary<string, string>
        {
            ["Subject"] = "Payment failed — action required",
            ["PaymentId"] = notification.PaymentId.ToString(),
            ["BookingId"] = notification.BookingId.ToString(),
            ["FailureReason"] = notification.FailureReason,
            ["FailedAt"] = notification.OccurredAt.ToString("yyyy-MM-dd HH:mm UTC")
        };

        var command = new SendNotificationCommand(
            null,
            NotificationChannel.Email,
            NotificationType.PaymentFailed,
            "guest@stayhub.local",
            "PaymentFailed",
            templateData,
            notification.BookingId);

        await _mediator.Send(command, cancellationToken);
    }
}
