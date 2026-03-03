namespace StayHub.Services.Notification.Domain.Enums;

/// <summary>
/// The delivery channel for a notification.
/// </summary>
public enum NotificationChannel
{
    /// <summary>Sent via SMTP / email provider.</summary>
    Email = 0,

    /// <summary>Stored as an in-app notification (future).</summary>
    InApp = 1,

    /// <summary>Push notification (future).</summary>
    Push = 2
}
