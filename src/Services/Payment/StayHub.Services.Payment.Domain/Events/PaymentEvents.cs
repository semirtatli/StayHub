using StayHub.Services.Payment.Domain.Enums;
using StayHub.Shared.Domain;

namespace StayHub.Services.Payment.Domain.Events;

/// <summary>
/// Raised when a new payment is created in Pending status.
/// Consumers: Initiate payment processing with the provider.
/// </summary>
public sealed record PaymentCreatedEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency) : DomainEvent;

/// <summary>
/// Raised when payment succeeds (money captured).
/// Consumers: Update booking status to Confirmed, send receipt email.
/// </summary>
public sealed record PaymentSucceededEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string ProviderTransactionId) : DomainEvent;

/// <summary>
/// Raised when payment processing fails.
/// Consumers: Notify guest, possibly expire booking.
/// </summary>
public sealed record PaymentFailedEvent(
    Guid PaymentId,
    Guid BookingId,
    string FailureReason) : DomainEvent;

/// <summary>
/// Raised when payment status changes. Generic event for audit trail.
/// </summary>
public sealed record PaymentStatusChangedEvent(
    Guid PaymentId,
    PaymentStatus OldStatus,
    PaymentStatus NewStatus) : DomainEvent;

/// <summary>
/// Raised when a refund is processed (full or partial).
/// Consumers: Update booking refund info, send refund notification.
/// </summary>
public sealed record RefundProcessedEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal RefundAmount,
    string Currency,
    bool IsFullRefund) : DomainEvent;
