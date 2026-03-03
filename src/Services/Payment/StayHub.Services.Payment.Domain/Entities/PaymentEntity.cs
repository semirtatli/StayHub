using StayHub.Services.Payment.Domain.Enums;
using StayHub.Services.Payment.Domain.Events;
using StayHub.Services.Payment.Domain.ValueObjects;
using StayHub.Shared.Domain;

namespace StayHub.Services.Payment.Domain.Entities;

/// <summary>
/// Payment aggregate root — represents a payment attempt for a booking.
///
/// A booking can have multiple payment attempts (e.g., first fails, second succeeds).
/// The payment tracks its lifecycle from creation through provider processing to completion.
///
/// State machine:
///   Pending → Processing → Succeeded → PartiallyRefunded → FullyRefunded
///                                    → FullyRefunded (direct full refund)
///   Pending → Processing → Failed
///   Pending → Cancelled
///
/// Invariants:
/// - Amount must be positive
/// - Refunded amount cannot exceed paid amount
/// - State transitions are validated (see each method)
/// </summary>
public sealed class PaymentEntity : AggregateRoot
{
    /// <summary>Reference to the booking this payment is for.</summary>
    public Guid BookingId { get; private init; }

    /// <summary>The user who is making the payment (guest).</summary>
    public string UserId { get; private init; } = null!;

    /// <summary>The total amount to be paid.</summary>
    public Money Amount { get; private init; } = null!;

    /// <summary>Current payment status.</summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>The payment method used.</summary>
    public PaymentMethod Method { get; private set; }

    /// <summary>
    /// External payment provider's transaction identifier.
    /// For Stripe: PaymentIntent ID (e.g., pi_3MtwBw...).
    /// Set when the provider responds.
    /// </summary>
    public string? ProviderTransactionId { get; private set; }

    /// <summary>
    /// Client secret for the frontend to complete payment (e.g., Stripe PaymentIntent client_secret).
    /// Only set during payment creation, used by the frontend SDK.
    /// </summary>
    public string? ClientSecret { get; private set; }

    /// <summary>Total amount refunded to the customer.</summary>
    public Money RefundedAmount { get; private set; } = null!;

    /// <summary>Reason for payment failure.</summary>
    public string? FailureReason { get; private set; }

    /// <summary>When the payment was successfully captured.</summary>
    public DateTime? PaidAt { get; private set; }

    /// <summary>When the payment failed.</summary>
    public DateTime? FailedAt { get; private set; }

    /// <summary>When the payment was cancelled.</summary>
    public DateTime? CancelledAt { get; private set; }

    // EF Core constructor
    private PaymentEntity()
    {
        UserId = null!;
        Amount = null!;
        RefundedAmount = null!;
    }

    /// <summary>
    /// Creates a new payment in Pending status.
    /// Called when a guest initiates payment for a booking.
    /// </summary>
    public static PaymentEntity Create(
        Guid bookingId,
        string userId,
        decimal amount,
        string currency,
        PaymentMethod method)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        var payment = new PaymentEntity
        {
            BookingId = bookingId,
            UserId = userId,
            Amount = Money.Create(amount, currency),
            Status = PaymentStatus.Pending,
            Method = method,
            RefundedAmount = Money.Zero(currency)
        };

        payment.RaiseDomainEvent(new PaymentCreatedEvent(
            payment.Id, bookingId, amount, currency));

        return payment;
    }

    /// <summary>
    /// Records the provider transaction details after payment initiation.
    /// Transitions: Pending → Processing.
    /// </summary>
    public void MarkAsProcessing(string providerTransactionId, string? clientSecret = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot start processing a {Status} payment.");

        ArgumentException.ThrowIfNullOrWhiteSpace(providerTransactionId);

        var oldStatus = Status;
        ProviderTransactionId = providerTransactionId;
        ClientSecret = clientSecret;
        Status = PaymentStatus.Processing;

        RaiseDomainEvent(new PaymentStatusChangedEvent(Id, oldStatus, Status));
    }

    /// <summary>
    /// Marks the payment as successfully captured.
    /// Transitions: Processing → Succeeded.
    /// </summary>
    public void MarkAsSucceeded(string providerTransactionId)
    {
        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot succeed a {Status} payment.");

        var oldStatus = Status;

        if (!string.IsNullOrWhiteSpace(providerTransactionId))
            ProviderTransactionId = providerTransactionId;

        Status = PaymentStatus.Succeeded;
        PaidAt = DateTime.UtcNow;

        RaiseDomainEvent(new PaymentStatusChangedEvent(Id, oldStatus, Status));
        RaiseDomainEvent(new PaymentSucceededEvent(
            Id, BookingId, Amount.Amount, Amount.Currency, ProviderTransactionId!));
    }

    /// <summary>
    /// Marks the payment as failed.
    /// Transitions: Processing → Failed, Pending → Failed.
    /// </summary>
    public void MarkAsFailed(string failureReason)
    {
        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot fail a {Status} payment.");

        var oldStatus = Status;
        FailureReason = failureReason;
        Status = PaymentStatus.Failed;
        FailedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PaymentStatusChangedEvent(Id, oldStatus, Status));
        RaiseDomainEvent(new PaymentFailedEvent(Id, BookingId, failureReason));
    }

    /// <summary>
    /// Cancels a pending payment (e.g., booking cancelled before payment processing).
    /// Transitions: Pending → Cancelled.
    /// </summary>
    public void Cancel()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot cancel a {Status} payment.");

        var oldStatus = Status;
        Status = PaymentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;

        RaiseDomainEvent(new PaymentStatusChangedEvent(Id, oldStatus, Status));
    }

    /// <summary>
    /// Process a refund (full or partial).
    /// Transitions: Succeeded → PartiallyRefunded or FullyRefunded.
    ///              PartiallyRefunded → PartiallyRefunded or FullyRefunded.
    /// </summary>
    public void ProcessRefund(decimal refundAmount)
    {
        if (Status != PaymentStatus.Succeeded && Status != PaymentStatus.PartiallyRefunded)
            throw new InvalidOperationException($"Cannot refund a {Status} payment.");

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(refundAmount);

        var maxRefundable = Amount.Amount - RefundedAmount.Amount;
        if (refundAmount > maxRefundable)
            throw new InvalidOperationException(
                $"Refund amount {refundAmount} exceeds maximum refundable {maxRefundable}.");

        var oldStatus = Status;
        var refundMoney = Money.Create(refundAmount, Amount.Currency);
        RefundedAmount = RefundedAmount.Add(refundMoney);

        var isFullRefund = RefundedAmount.Amount >= Amount.Amount;
        Status = isFullRefund ? PaymentStatus.FullyRefunded : PaymentStatus.PartiallyRefunded;

        RaiseDomainEvent(new PaymentStatusChangedEvent(Id, oldStatus, Status));
        RaiseDomainEvent(new RefundProcessedEvent(
            Id, BookingId, refundAmount, Amount.Currency, isFullRefund));
    }

    /// <summary>Whether this payment can still be refunded.</summary>
    public bool CanRefund => Status is PaymentStatus.Succeeded or PaymentStatus.PartiallyRefunded;

    /// <summary>Remaining amount available for refund.</summary>
    public decimal RefundableAmount => Amount.Amount - RefundedAmount.Amount;
}
