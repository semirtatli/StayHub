namespace StayHub.Services.Booking.Domain.Enums;

/// <summary>
/// Payment status for a booking.
/// Tracked separately from booking status to handle async payment flows.
/// </summary>
public enum PaymentStatus
{
    /// <summary>Payment not yet initiated.</summary>
    Pending = 0,

    /// <summary>Payment is being processed (e.g., 3D Secure).</summary>
    Processing = 1,

    /// <summary>Payment successfully captured.</summary>
    Paid = 2,

    /// <summary>Payment failed — booking will not be confirmed.</summary>
    Failed = 3,

    /// <summary>Full refund issued.</summary>
    Refunded = 4,

    /// <summary>Partial refund issued (late cancellation).</summary>
    PartiallyRefunded = 5
}
