using StayHub.Services.Notification.Domain.Enums;
using StayHub.Services.Notification.Domain.Events;
using StayHub.Shared.Domain;

namespace StayHub.Services.Notification.Domain.Entities;

/// <summary>
/// Represents a notification that was (or will be) sent to a user.
/// Tracks delivery status, retry count, and the rendered content.
///
/// Each notification is an immutable record of a communication attempt.
/// The aggregate root controls status transitions and retry logic.
///
/// Invariants:
/// - MaxRetries is 3 — after that, the notification is permanently failed.
/// - FailureReason is only set when Status == Failed.
/// - SentAt is only set when Status == Sent.
/// </summary>
public sealed class NotificationEntity : AggregateRoot
{
    private const int MaxRetries = 3;

    /// <summary>The user this notification is for (may be null for system notifications).</summary>
    public string? UserId { get; private init; }

    /// <summary>Delivery channel.</summary>
    public NotificationChannel Channel { get; private init; }

    /// <summary>Business event type that triggered this notification.</summary>
    public NotificationType Type { get; private init; }

    /// <summary>Recipient address (email, device token, etc.).</summary>
    public string Recipient { get; private init; } = null!;

    /// <summary>Subject line (for email) or title (for push).</summary>
    public string Subject { get; private init; } = null!;

    /// <summary>Rendered notification body (HTML for email, text for push).</summary>
    public string Body { get; private init; } = null!;

    /// <summary>Current delivery status.</summary>
    public NotificationStatus Status { get; private set; }

    /// <summary>When the notification was successfully sent.</summary>
    public DateTime? SentAt { get; private set; }

    /// <summary>When the last failure occurred.</summary>
    public DateTime? FailedAt { get; private set; }

    /// <summary>Reason for the last failure.</summary>
    public string? FailureReason { get; private set; }

    /// <summary>How many delivery attempts have been made.</summary>
    public int RetryCount { get; private set; }

    /// <summary>Optional correlation ID linking to the source event (e.g., BookingId).</summary>
    public Guid? CorrelationId { get; private init; }

    // EF Core constructor
    private NotificationEntity() { }

    /// <summary>
    /// Creates a new notification in Pending status.
    /// </summary>
    public static NotificationEntity Create(
        string? userId,
        NotificationChannel channel,
        NotificationType type,
        string recipient,
        string subject,
        string body,
        Guid? correlationId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        return new NotificationEntity
        {
            UserId = userId,
            Channel = channel,
            Type = type,
            Recipient = recipient,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.Pending,
            RetryCount = 0,
            CorrelationId = correlationId
        };
    }

    /// <summary>
    /// Marks the notification as successfully sent.
    /// </summary>
    public void MarkAsSent()
    {
        if (Status == NotificationStatus.Sent)
            return;

        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;

        RaiseDomainEvent(new NotificationSentEvent(
            Id, Recipient, Type.ToString()));
    }

    /// <summary>
    /// Records a delivery failure. If max retries exceeded, marks as permanently failed.
    /// </summary>
    public void MarkAsFailed(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        RetryCount++;
        FailedAt = DateTime.UtcNow;
        FailureReason = reason;

        if (RetryCount >= MaxRetries)
        {
            Status = NotificationStatus.Failed;

            RaiseDomainEvent(new NotificationFailedEvent(
                Id, Recipient, Type.ToString(), reason));
        }
    }

    /// <summary>
    /// Whether this notification can still be retried.
    /// </summary>
    public bool CanRetry => Status != NotificationStatus.Sent && RetryCount < MaxRetries;
}
