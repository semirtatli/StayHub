using StayHub.Shared.Result;

namespace StayHub.Services.Notification.Application;

/// <summary>
/// Centralized error definitions for the Notification Service.
/// </summary>
public static class NotificationErrors
{
    public static class Notification
    {
        public static readonly Error NotFound = new(
            "Notification.NotFound",
            "The notification was not found.");

        public static readonly Error AlreadySent = new(
            "Notification.AlreadySent",
            "The notification has already been sent.");

        public static readonly Error MaxRetriesExceeded = new(
            "Notification.MaxRetriesExceeded",
            "The notification has exceeded the maximum number of retries.");

        public static readonly Error SendFailed = new(
            "Notification.SendFailed",
            "Failed to send the notification.");

        public static readonly Error InvalidRecipient = new(
            "Notification.InvalidRecipient",
            "The recipient address is invalid.");

        public static readonly Error TemplateNotFound = new(
            "Notification.TemplateNotFound",
            "The email template was not found.");
    }
}
