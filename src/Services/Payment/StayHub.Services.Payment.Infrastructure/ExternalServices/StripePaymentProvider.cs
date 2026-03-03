using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StayHub.Services.Payment.Application.Abstractions;
using Stripe;
using DomainPaymentMethod = StayHub.Services.Payment.Domain.Enums.PaymentMethod;

namespace StayHub.Services.Payment.Infrastructure.ExternalServices;

/// <summary>
/// Stripe implementation of IPaymentProvider.
/// Uses the Stripe.net SDK to create PaymentIntents, process refunds, and validate webhooks.
/// </summary>
public sealed class StripePaymentProvider : IPaymentProvider
{
    private readonly ILogger<StripePaymentProvider> _logger;
    private readonly string _webhookSecret;

    public StripePaymentProvider(
        IConfiguration configuration,
        ILogger<StripePaymentProvider> logger)
    {
        _logger = logger;

        var secretKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");

        _webhookSecret = configuration["Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe:WebhookSecret is not configured.");

        // Set the global API key for the Stripe SDK
        StripeConfiguration.ApiKey = secretKey;
    }

    /// <inheritdoc />
    public async Task<PaymentProviderResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        DomainPaymentMethod method,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = ConvertToStripeAmount(amount, currency),
                Currency = currency.ToLowerInvariant(),
                PaymentMethodTypes = MapPaymentMethod(method),
                Metadata = metadata
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Stripe PaymentIntent created: {IntentId} for {Amount} {Currency}",
                intent.Id, amount, currency);

            return new PaymentProviderResult(
                IsSuccess: true,
                ProviderTransactionId: intent.Id,
                ClientSecret: intent.ClientSecret,
                ErrorMessage: null);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex,
                "Stripe PaymentIntent creation failed: {Message}",
                ex.Message);

            return new PaymentProviderResult(
                IsSuccess: false,
                ProviderTransactionId: null,
                ClientSecret: null,
                ErrorMessage: ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<RefundProviderResult> RefundAsync(
        string providerTransactionId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = providerTransactionId,
                Amount = ConvertToStripeAmount(amount, currency)
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Stripe refund processed: {RefundId} for {Amount} {Currency}",
                refund.Id, amount, currency);

            return new RefundProviderResult(
                IsSuccess: true,
                RefundId: refund.Id,
                RefundedAmount: amount,
                ErrorMessage: null);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex,
                "Stripe refund failed for {ProviderTxnId}: {Message}",
                providerTransactionId, ex.Message);

            return new RefundProviderResult(
                IsSuccess: false,
                RefundId: null,
                RefundedAmount: 0,
                ErrorMessage: ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<bool> CancelPaymentIntentAsync(
        string providerTransactionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService();
            await service.CancelAsync(providerTransactionId, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Stripe PaymentIntent cancelled: {IntentId}",
                providerTransactionId);

            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex,
                "Stripe PaymentIntent cancellation failed for {IntentId}: {Message}",
                providerTransactionId, ex.Message);

            return false;
        }
    }

    /// <inheritdoc />
    public WebhookEvent? ValidateWebhookSignature(string payload, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, _webhookSecret);

            // Extract the PaymentIntent from the event data
            if (stripeEvent.Data.Object is not PaymentIntent intent)
            {
                _logger.LogWarning(
                    "Webhook event {EventType} does not contain a PaymentIntent",
                    stripeEvent.Type);
                return null;
            }

            return new WebhookEvent(
                EventType: stripeEvent.Type,
                ProviderTransactionId: intent.Id,
                FailureReason: intent.LastPaymentError?.Message,
                Amount: intent.Amount > 0
                    ? ConvertFromStripeAmount(intent.Amount, intent.Currency)
                    : null);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Webhook signature validation failed: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Converts a decimal amount to Stripe's smallest currency unit (e.g., cents for USD).
    /// Stripe expects amounts in the smallest unit (e.g., 10.50 USD → 1050).
    /// </summary>
    private static long ConvertToStripeAmount(decimal amount, string currency)
    {
        // Zero-decimal currencies (JPY, KRW, etc.) don't need multiplication
        var zeroDecimalCurrencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BIF", "CLP", "DJF", "GNF", "JPY", "KMF", "KRW", "MGA",
            "PYG", "RWF", "UGX", "VND", "VUV", "XAF", "XOF", "XPF"
        };

        return zeroDecimalCurrencies.Contains(currency)
            ? (long)amount
            : (long)(amount * 100);
    }

    /// <summary>
    /// Converts a Stripe amount back to a decimal (e.g., 1050 → 10.50 USD).
    /// </summary>
    private static decimal ConvertFromStripeAmount(long amount, string currency)
    {
        var zeroDecimalCurrencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BIF", "CLP", "DJF", "GNF", "JPY", "KMF", "KRW", "MGA",
            "PYG", "RWF", "UGX", "VND", "VUV", "XAF", "XOF", "XPF"
        };

        return zeroDecimalCurrencies.Contains(currency)
            ? amount
            : amount / 100m;
    }

    /// <summary>
    /// Maps our PaymentMethod enum to Stripe's payment method type strings.
    /// </summary>
    private static List<string> MapPaymentMethod(DomainPaymentMethod method)
    {
        return method switch
        {
            DomainPaymentMethod.CreditCard or DomainPaymentMethod.DebitCard => ["card"],
            DomainPaymentMethod.BankTransfer => ["us_bank_account"],
            _ => ["card"]
        };
    }
}
