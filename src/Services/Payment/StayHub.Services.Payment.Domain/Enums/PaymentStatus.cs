namespace StayHub.Services.Payment.Domain.Enums;

/// <summary>
/// Payment status — tracks the lifecycle of a payment from creation to completion/refund.
///
/// State machine:
///   Pending → Processing → Succeeded → (PartiallyRefunded | FullyRefunded)
///   Pending → Processing → Failed
///   Pending → Cancelled
/// </summary>
public enum PaymentStatus
{
    /// <summary>Payment created, awaiting processing.</summary>
    Pending = 0,

    /// <summary>Payment is being processed by the payment provider (e.g., 3D Secure in progress).</summary>
    Processing = 1,

    /// <summary>Payment successfully captured.</summary>
    Succeeded = 2,

    /// <summary>Payment processing failed (card declined, insufficient funds, etc.).</summary>
    Failed = 3,

    /// <summary>Payment cancelled before processing (e.g., booking cancelled while Pending).</summary>
    Cancelled = 4,

    /// <summary>Part of the payment has been refunded.</summary>
    PartiallyRefunded = 5,

    /// <summary>The full payment has been refunded.</summary>
    FullyRefunded = 6
}
