using System.Globalization;
using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Services.Notification.Application.Features.SendNotification;
using StayHub.Services.Notification.Application.IntegrationEvents;
using StayHub.Services.Notification.Domain.Enums;

namespace StayHub.Services.Notification.Application.Consumers;

/// <summary>
/// Consumes PaymentSucceededIntegrationEvent and sends a payment receipt email.
/// </summary>
public sealed class PaymentSucceededConsumer : INotificationHandler<PaymentSucceededIntegrationEvent>
{
    private readonly ISender _mediator;
    private readonly ILogger<PaymentSucceededConsumer> _logger;

    public PaymentSucceededConsumer(ISender mediator, ILogger<PaymentSucceededConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(PaymentSucceededIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing payment receipt for PaymentId={PaymentId}, BookingId={BookingId}, Amount={Amount} {Currency}",
            notification.PaymentId, notification.BookingId, notification.Amount, notification.Currency);

        var templateData = new Dictionary<string, string>
        {
            ["Subject"] = "Payment receipt — StayHub",
            ["PaymentId"] = notification.PaymentId.ToString(),
            ["BookingId"] = notification.BookingId.ToString(),
            ["Amount"] = notification.Amount.ToString("F2", CultureInfo.InvariantCulture),
            ["Currency"] = notification.Currency,
            ["PaidAt"] = notification.OccurredAt.ToString("yyyy-MM-dd HH:mm UTC")
        };

        var command = new SendNotificationCommand(
            null,
            NotificationChannel.Email,
            NotificationType.PaymentReceipt,
            "guest@stayhub.local",
            "PaymentReceipt",
            templateData,
            notification.BookingId);

        await _mediator.Send(command, cancellationToken);
    }
}
