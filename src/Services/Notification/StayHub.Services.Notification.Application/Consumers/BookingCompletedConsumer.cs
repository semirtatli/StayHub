using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Services.Notification.Application.Features.SendNotification;
using StayHub.Services.Notification.Application.IntegrationEvents;
using StayHub.Services.Notification.Domain.Enums;

namespace StayHub.Services.Notification.Application.Consumers;

/// <summary>
/// Consumes BookingCompletedIntegrationEvent and sends a review reminder email.
/// Triggered when a guest checks out — encourages them to leave a review.
/// </summary>
public sealed class BookingCompletedConsumer : INotificationHandler<BookingCompletedIntegrationEvent>
{
    private readonly ISender _mediator;
    private readonly ILogger<BookingCompletedConsumer> _logger;

    public BookingCompletedConsumer(ISender mediator, ILogger<BookingCompletedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(BookingCompletedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing review reminder for BookingId={BookingId}, Guest={GuestUserId}",
            notification.BookingId, notification.GuestUserId);

        var templateData = new Dictionary<string, string>
        {
            ["Subject"] = "How was your stay? Leave a review!",
            ["BookingId"] = notification.BookingId.ToString(),
            ["HotelId"] = notification.HotelId.ToString(),
            ["GuestUserId"] = notification.GuestUserId,
            ["CompletedAt"] = notification.OccurredAt.ToString("yyyy-MM-dd HH:mm UTC")
        };

        var command = new SendNotificationCommand(
            notification.GuestUserId,
            NotificationChannel.Email,
            NotificationType.ReviewReminder,
            $"{notification.GuestUserId}@stayhub.local",
            "ReviewReminder",
            templateData,
            notification.BookingId);

        await _mediator.Send(command, cancellationToken);
    }
}
