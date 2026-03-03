using System.Globalization;
using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Services.Notification.Application.Features.SendNotification;
using StayHub.Services.Notification.Application.IntegrationEvents;
using StayHub.Services.Notification.Domain.Enums;

namespace StayHub.Services.Notification.Application.Consumers;

/// <summary>
/// Consumes RefundProcessedIntegrationEvent and sends a refund notification email.
/// </summary>
public sealed class RefundProcessedConsumer : INotificationHandler<RefundProcessedIntegrationEvent>
{
    private readonly ISender _mediator;
    private readonly ILogger<RefundProcessedConsumer> _logger;

    public RefundProcessedConsumer(ISender mediator, ILogger<RefundProcessedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(RefundProcessedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing refund notification for PaymentId={PaymentId}, Amount={Amount} {Currency}, IsFullRefund={IsFullRefund}",
            notification.PaymentId, notification.RefundAmount, notification.Currency, notification.IsFullRefund);

        var templateData = new Dictionary<string, string>
        {
            ["Subject"] = notification.IsFullRefund ? "Full refund processed" : "Partial refund processed",
            ["PaymentId"] = notification.PaymentId.ToString(),
            ["BookingId"] = notification.BookingId.ToString(),
            ["RefundAmount"] = notification.RefundAmount.ToString("F2", CultureInfo.InvariantCulture),
            ["Currency"] = notification.Currency,
            ["IsFullRefund"] = notification.IsFullRefund.ToString(),
            ["ProcessedAt"] = notification.OccurredAt.ToString("yyyy-MM-dd HH:mm UTC")
        };

        var command = new SendNotificationCommand(
            null,
            NotificationChannel.Email,
            NotificationType.RefundProcessed,
            "guest@stayhub.local",
            "RefundProcessed",
            templateData,
            notification.BookingId);

        await _mediator.Send(command, cancellationToken);
    }
}
