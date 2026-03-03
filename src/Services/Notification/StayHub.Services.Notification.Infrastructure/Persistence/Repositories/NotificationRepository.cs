using Microsoft.EntityFrameworkCore;
using StayHub.Services.Notification.Domain.Entities;
using StayHub.Services.Notification.Domain.Enums;
using StayHub.Services.Notification.Domain.Repositories;
using StayHub.Shared.Infrastructure.Persistence;

namespace StayHub.Services.Notification.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of INotificationRepository.
/// </summary>
public sealed class NotificationRepository : Repository<NotificationEntity>, INotificationRepository
{
    public NotificationRepository(NotificationDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NotificationEntity>> GetByUserIdAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NotificationEntity>> GetByStatusAsync(
        NotificationStatus status, int maxCount = 50, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.Status == status)
            .OrderBy(n => n.CreatedAt)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NotificationEntity>> GetByCorrelationIdAsync(
        Guid correlationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.CorrelationId == correlationId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NotificationEntity>> GetRetryableAsync(
        int maxCount = 20, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.Status == NotificationStatus.Pending && n.RetryCount > 0 && n.RetryCount < 3)
            .OrderBy(n => n.CreatedAt)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }
}
