using Microsoft.Extensions.Logging;
using StayHub.Services.Notification.Application.Abstractions;
using StayHub.Services.Notification.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Notification.Application.Features.RetryFailedNotifications;

/// <summary>
/// Retries all retryable notifications by re-sending them through the email sender.
/// Each notification tracks its own retry count — permanently fails after max retries.
/// </summary>
internal sealed class RetryFailedNotificationsCommandHandler : ICommandHandler<RetryFailedNotificationsCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RetryFailedNotificationsCommandHandler> _logger;

    public RetryFailedNotificationsCommandHandler(
        INotificationRepository notificationRepository,
        IEmailSender emailSender,
        ILogger<RetryFailedNotificationsCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Result> Handle(
        RetryFailedNotificationsCommand request, CancellationToken cancellationToken)
    {
        var retryable = await _notificationRepository.GetRetryableAsync(
            maxCount: 20, cancellationToken: cancellationToken);

        if (retryable.Count == 0)
        {
            _logger.LogDebug("No retryable notifications found.");
            return Result.Success();
        }

        _logger.LogInformation("Retrying {Count} failed notification(s)", retryable.Count);

        foreach (var notification in retryable)
        {
            try
            {
                var sent = await _emailSender.SendAsync(
                    notification.Recipient, notification.Subject, notification.Body, cancellationToken);

                if (sent)
                {
                    notification.MarkAsSent();
                    _logger.LogInformation(
                        "Retry succeeded for notification {NotificationId}", notification.Id);
                }
                else
                {
                    notification.MarkAsFailed("Retry: email sender returned failure.");
                }
            }
            catch (Exception ex)
            {
                notification.MarkAsFailed($"Retry failed: {ex.Message}");
                _logger.LogWarning(ex,
                    "Retry failed for notification {NotificationId}", notification.Id);
            }

            _notificationRepository.Update(notification);
        }

        return Result.Success();
    }
}
