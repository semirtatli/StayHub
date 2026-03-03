using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Services.Notification.Application.Features.SendNotification;
using StayHub.Services.Notification.Application.IntegrationEvents;
using StayHub.Services.Notification.Domain.Enums;

namespace StayHub.Services.Notification.Application.Consumers;

/// <summary>
/// Consumes BookingCancelledIntegrationEvent and sends a cancellation email.
/// </summary>
public sealed class BookingCancelledConsumer : INotificationHandler<BookingCancelledIntegrationEvent>
{
    private readonly ISender _mediator;
    private readonly ILogger<BookingCancelledConsumer> _logger;

    public BookingCancelledConsumer(ISender mediator, ILogger<BookingCancelledConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(BookingCancelledIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing booking cancellation for BookingId={BookingId}",
            notification.BookingId);

        var templateData = new Dictionary<string, string>
        {
            ["Subject"] = "Your booking has been cancelled",
            ["BookingId"] = notification.BookingId.ToString(),
            ["HotelId"] = notification.HotelId.ToString(),
            ["CancellationReason"] = notification.CancellationReason ?? "No reason provided",
            ["CancelledAt"] = notification.OccurredAt.ToString("yyyy-MM-dd HH:mm UTC")
        };

        // Note: In production, we'd resolve the guest email from the booking or user service.
        var command = new SendNotificationCommand(
            null,
            NotificationChannel.Email,
            NotificationType.BookingCancellation,
            "guest@stayhub.local",
            "BookingCancellation",
            templateData,
            notification.BookingId);

        await _mediator.Send(command, cancellationToken);
    }
}
