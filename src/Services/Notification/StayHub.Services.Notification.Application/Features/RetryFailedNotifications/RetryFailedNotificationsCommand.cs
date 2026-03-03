using StayHub.Shared.CQRS;

namespace StayHub.Services.Notification.Application.Features.RetryFailedNotifications;

/// <summary>
/// Command to retry sending failed notifications that haven't exceeded max retries.
/// Triggered periodically by a background service or admin endpoint.
/// </summary>
public sealed record RetryFailedNotificationsCommand : ICommand;
