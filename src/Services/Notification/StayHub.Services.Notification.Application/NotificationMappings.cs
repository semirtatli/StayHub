using StayHub.Services.Notification.Application.DTOs;
using StayHub.Services.Notification.Domain.Entities;

namespace StayHub.Services.Notification.Application;

/// <summary>
/// Mapping extensions for Notification entities to DTOs.
/// </summary>
public static class NotificationMappings
{
    public static NotificationDto ToDto(this NotificationEntity entity)
    {
        return new NotificationDto(
            entity.Id,
            entity.UserId,
            entity.Channel,
            entity.Type,
            entity.Recipient,
            entity.Subject,
            entity.Status,
            entity.SentAt,
            entity.FailedAt,
            entity.FailureReason,
            entity.RetryCount,
            entity.CorrelationId,
            entity.CreatedAt);
    }

    public static NotificationSummaryDto ToSummaryDto(this NotificationEntity entity)
    {
        return new NotificationSummaryDto(
            entity.Id,
            entity.Type,
            entity.Recipient,
            entity.Subject,
            entity.Status,
            entity.CreatedAt);
    }
}
