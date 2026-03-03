using StayHub.Services.Notification.Domain.Entities;
using StayHub.Services.Notification.Domain.Enums;
using StayHub.Shared.Interfaces;

namespace StayHub.Services.Notification.Domain.Repositories;

/// <summary>
/// Repository interface for the Notification aggregate.
/// </summary>
public interface INotificationRepository : IRepository<NotificationEntity>
{
    /// <summary>Get all notifications for a specific user.</summary>
    Task<IReadOnlyList<NotificationEntity>> GetByUserIdAsync(
        string userId, CancellationToken cancellationToken = default);

    /// <summary>Get notifications by status (for retry processing).</summary>
    Task<IReadOnlyList<NotificationEntity>> GetByStatusAsync(
        NotificationStatus status, int maxCount = 50, CancellationToken cancellationToken = default);

    /// <summary>Get notifications by correlation ID (e.g., all notifications for a booking).</summary>
    Task<IReadOnlyList<NotificationEntity>> GetByCorrelationIdAsync(
        Guid correlationId, CancellationToken cancellationToken = default);

    /// <summary>Get pending notifications that can still be retried.</summary>
    Task<IReadOnlyList<NotificationEntity>> GetRetryableAsync(
        int maxCount = 20, CancellationToken cancellationToken = default);
}
