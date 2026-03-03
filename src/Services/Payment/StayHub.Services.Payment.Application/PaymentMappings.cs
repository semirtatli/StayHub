using StayHub.Services.Payment.Application.DTOs;
using StayHub.Services.Payment.Domain.Entities;

namespace StayHub.Services.Payment.Application;

/// <summary>
/// Manual mapping extensions from domain entities to DTOs.
/// Using manual mappings for full control and no reflection overhead.
/// </summary>
public static class PaymentMappings
{
    public static PaymentDto ToDto(this PaymentEntity payment)
    {
        return new PaymentDto(
            payment.Id,
            payment.BookingId,
            payment.UserId,
            payment.Amount.Amount,
            payment.Amount.Currency,
            payment.Status.ToString(),
            payment.Method.ToString(),
            payment.ProviderTransactionId,
            payment.ClientSecret,
            payment.RefundedAmount.Amount,
            payment.FailureReason,
            payment.PaidAt,
            payment.FailedAt,
            payment.CancelledAt,
            payment.CreatedAt);
    }

    public static PaymentSummaryDto ToSummaryDto(this PaymentEntity payment)
    {
        return new PaymentSummaryDto(
            payment.Id,
            payment.BookingId,
            payment.Amount.Amount,
            payment.Amount.Currency,
            payment.Status.ToString(),
            payment.Method.ToString(),
            payment.RefundedAmount.Amount,
            payment.PaidAt,
            payment.CreatedAt);
    }
}
