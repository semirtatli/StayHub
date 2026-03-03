using StayHub.Shared.Result;

namespace StayHub.Services.Payment.Application;

/// <summary>
/// Static error definitions for the Payment bounded context.
/// Follows the pattern: "{Entity}.{ErrorType}" for consistent error codes.
/// </summary>
public static class PaymentErrors
{
    public static class Payment
    {
        public static readonly Error NotFound = new(
            "Payment.NotFound",
            "Payment was not found.");

        public static readonly Error AlreadyProcessed = new(
            "Payment.AlreadyProcessed",
            "Payment has already been processed.");

        public static readonly Error InvalidStatus = new(
            "Payment.InvalidStatus",
            "The operation is not allowed for the payment's current status.");

        public static readonly Error InvalidStatusTransition = new(
            "Payment.InvalidStatusTransition",
            "This status transition is not allowed for the payment's current status.");

        public static readonly Error ProviderError = new(
            "Payment.ProviderError",
            "The payment provider returned an error.");

        public static readonly Error RefundNotAllowed = new(
            "Payment.RefundNotAllowed",
            "Refund is not allowed for this payment.");

        public static readonly Error RefundExceedsAmount = new(
            "Payment.RefundExceedsAmount",
            "Refund amount exceeds the refundable amount.");

        public static readonly Error BookingNotFound = new(
            "Payment.BookingNotFound",
            "No payment found for the specified booking.");

        public static readonly Error DuplicateWebhookEvent = new(
            "Payment.DuplicateWebhookEvent",
            "This webhook event has already been processed.");

        public static readonly Error InvalidWebhookSignature = new(
            "Payment.InvalidWebhookSignature",
            "The webhook signature is invalid.");

        public static readonly Error NotOwner = new(
            "Payment.NotOwner",
            "You are not the owner of this payment.");
    }
}
