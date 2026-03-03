using StayHub.Shared.CQRS;

namespace StayHub.Services.Payment.Application.Features.ProcessWebhook;

/// <summary>
/// Command to process a webhook event from the payment provider (Stripe).
///
/// The controller receives the raw JSON payload + signature header,
/// passes them here for validation and processing.
/// No authentication required — verified by webhook signature.
/// </summary>
public sealed record ProcessWebhookCommand(
    string Payload,
    string Signature) : ICommand;
