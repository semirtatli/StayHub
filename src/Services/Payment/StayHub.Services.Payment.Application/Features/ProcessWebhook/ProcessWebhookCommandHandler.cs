using Microsoft.Extensions.Logging;
using StayHub.Services.Payment.Application.Abstractions;
using StayHub.Services.Payment.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Payment.Application.Features.ProcessWebhook;

/// <summary>
/// Handles Stripe webhook events — updates payment status based on the event type.
///
/// Supported events:
/// - payment_intent.succeeded → MarkAsSucceeded
/// - payment_intent.payment_failed → MarkAsFailed
/// - payment_intent.canceled → Cancel
///
/// TransactionBehavior commits the unit of work after a successful result.
/// </summary>
public sealed class ProcessWebhookCommandHandler : ICommandHandler<ProcessWebhookCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentProvider _paymentProvider;
    private readonly ILogger<ProcessWebhookCommandHandler> _logger;

    public ProcessWebhookCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentProvider paymentProvider,
        ILogger<ProcessWebhookCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _paymentProvider = paymentProvider;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ProcessWebhookCommand request,
        CancellationToken cancellationToken)
    {
        // Validate webhook signature
        var webhookEvent = _paymentProvider.ValidateWebhookSignature(
            request.Payload, request.Signature);

        if (webhookEvent is null)
            return Result.Failure(PaymentErrors.Payment.InvalidWebhookSignature);

        // Find the payment by provider transaction ID
        var payment = await _paymentRepository.GetByProviderTransactionIdAsync(
            webhookEvent.ProviderTransactionId, cancellationToken);

        if (payment is null)
        {
            _logger.LogWarning(
                "Webhook received for unknown provider transaction: {ProviderTxnId}",
                webhookEvent.ProviderTransactionId);
            return Result.Failure(PaymentErrors.Payment.NotFound);
        }

        try
        {
            switch (webhookEvent.EventType)
            {
                case "payment_intent.succeeded":
                    payment.MarkAsSucceeded(webhookEvent.ProviderTransactionId);
                    _logger.LogInformation(
                        "Payment {PaymentId} succeeded via webhook — Booking: {BookingId}",
                        payment.Id, payment.BookingId);
                    break;

                case "payment_intent.payment_failed":
                    payment.MarkAsFailed(webhookEvent.FailureReason ?? "Payment failed via provider.");
                    _logger.LogWarning(
                        "Payment {PaymentId} failed via webhook — Reason: {Reason}",
                        payment.Id, webhookEvent.FailureReason);
                    break;

                case "payment_intent.canceled":
                    payment.Cancel();
                    _logger.LogInformation(
                        "Payment {PaymentId} cancelled via webhook",
                        payment.Id);
                    break;

                default:
                    _logger.LogInformation(
                        "Webhook event type {EventType} is not handled — skipping",
                        webhookEvent.EventType);
                    return Result.Success();
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Webhook event type {EventType} caused invalid state transition for payment {PaymentId}: {Message}",
                webhookEvent.EventType, payment.Id, ex.Message);
            return Result.Failure(PaymentErrors.Payment.InvalidStatusTransition);
        }

        _paymentRepository.Update(payment);

        return Result.Success();
    }
}
