using StayHub.Services.Notification.Domain.Enums;

namespace StayHub.Services.Notification.Application.DTOs;

/// <summary>
/// Full notification details.
/// </summary>
public sealed record NotificationDto(
    Guid Id,
    string? UserId,
    NotificationChannel Channel,
    NotificationType Type,
    string Recipient,
    string Subject,
    NotificationStatus Status,
    DateTime? SentAt,
    DateTime? FailedAt,
    string? FailureReason,
    int RetryCount,
    Guid? CorrelationId,
    DateTime CreatedAt);

/// <summary>
/// Lightweight notification summary for listing.
/// </summary>
public sealed record NotificationSummaryDto(
    Guid Id,
    NotificationType Type,
    string Recipient,
    string Subject,
    NotificationStatus Status,
    DateTime CreatedAt);
