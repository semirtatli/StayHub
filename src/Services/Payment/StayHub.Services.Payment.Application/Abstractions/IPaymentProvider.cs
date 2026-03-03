using StayHub.Services.Payment.Domain.Enums;

namespace StayHub.Services.Payment.Application.Abstractions;

/// <summary>
/// Abstraction over the external payment provider (e.g., Stripe).
/// Infrastructure implements this with the concrete provider SDK.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Creates a payment intent with the provider and returns the result.
    /// </summary>
    Task<PaymentProviderResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        PaymentMethod method,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a refund for a previously succeeded payment.
    /// </summary>
    Task<RefundProviderResult> RefundAsync(
        string providerTransactionId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a payment intent before it's captured.
    /// </summary>
    Task<bool> CancelPaymentIntentAsync(
        string providerTransactionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a webhook signature from the provider.
    /// Returns the deserialized event if valid, null if invalid.
    /// </summary>
    WebhookEvent? ValidateWebhookSignature(string payload, string signature);
}

/// <summary>
/// Result of creating a payment intent with the external provider.
/// </summary>
public sealed record PaymentProviderResult(
    bool IsSuccess,
    string? ProviderTransactionId,
    string? ClientSecret,
    string? ErrorMessage);

/// <summary>
/// Result of processing a refund with the external provider.
/// </summary>
public sealed record RefundProviderResult(
    bool IsSuccess,
    string? RefundId,
    decimal RefundedAmount,
    string? ErrorMessage);

/// <summary>
/// Parsed webhook event from the payment provider.
/// </summary>
public sealed record WebhookEvent(
    string EventType,
    string ProviderTransactionId,
    string? FailureReason,
    decimal? Amount);
