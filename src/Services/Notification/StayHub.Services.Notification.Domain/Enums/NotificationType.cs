namespace StayHub.Services.Notification.Domain.Enums;

/// <summary>
/// Categorizes the notification by the business event that triggered it.
/// Used for template selection and analytics.
/// </summary>
public enum NotificationType
{
    /// <summary>Booking has been confirmed after payment.</summary>
    BookingConfirmation = 0,

    /// <summary>Booking has been cancelled.</summary>
    BookingCancellation = 1,

    /// <summary>Payment has been successfully processed.</summary>
    PaymentReceipt = 2,

    /// <summary>Payment processing has failed.</summary>
    PaymentFailed = 3,

    /// <summary>A refund has been processed.</summary>
    RefundProcessed = 4,

    /// <summary>Reminder to leave a review after checkout.</summary>
    ReviewReminder = 5,

    /// <summary>Welcome email after registration.</summary>
    WelcomeEmail = 6,

    /// <summary>Guest has checked in — welcome message.</summary>
    CheckInConfirmation = 7
}
