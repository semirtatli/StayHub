namespace StayHub.Services.Notification.Domain.Enums;

/// <summary>
/// Tracks the delivery lifecycle of a notification.
/// </summary>
public enum NotificationStatus
{
    /// <summary>Created but not yet sent.</summary>
    Pending = 0,

    /// <summary>Successfully delivered to the channel.</summary>
    Sent = 1,

    /// <summary>Delivery failed (may be retried).</summary>
    Failed = 2
}
