using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Services.Notification.Application.Features.SendNotification;
using StayHub.Services.Notification.Application.IntegrationEvents;
using StayHub.Services.Notification.Domain.Enums;

namespace StayHub.Services.Notification.Application.Consumers;

/// <summary>
/// Consumes BookingConfirmedIntegrationEvent and sends a booking confirmation email.
/// </summary>
public sealed class BookingConfirmedConsumer : INotificationHandler<BookingConfirmedIntegrationEvent>
{
    private readonly ISender _mediator;
    private readonly ILogger<BookingConfirmedConsumer> _logger;

    public BookingConfirmedConsumer(ISender mediator, ILogger<BookingConfirmedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(BookingConfirmedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing booking confirmation for BookingId={BookingId}, Guest={GuestUserId}",
            notification.BookingId, notification.GuestUserId);

        // In a real system, we'd look up the user's email from a user service or cache.
        // For now, use the userId as a placeholder email recipient.
        var templateData = new Dictionary<string, string>
        {
            ["Subject"] = "Your booking has been confirmed!",
            ["BookingId"] = notification.BookingId.ToString(),
            ["HotelId"] = notification.HotelId.ToString(),
            ["GuestUserId"] = notification.GuestUserId,
            ["ConfirmedAt"] = notification.OccurredAt.ToString("yyyy-MM-dd HH:mm UTC")
        };

        var command = new SendNotificationCommand(
            notification.GuestUserId,
            NotificationChannel.Email,
            NotificationType.BookingConfirmation,
            $"{notification.GuestUserId}@stayhub.local",
            "BookingConfirmation",
            templateData,
            notification.BookingId);

        await _mediator.Send(command, cancellationToken);
    }
}
