namespace StayHub.Services.Payment.Application.DTOs;

/// <summary>
/// Full payment detail DTO for API responses.
/// </summary>
public sealed record PaymentDto(
    Guid Id,
    Guid BookingId,
    string UserId,
    decimal Amount,
    string Currency,
    string Status,
    string Method,
    string? ProviderTransactionId,
    string? ClientSecret,
    decimal RefundedAmount,
    string? FailureReason,
    DateTime? PaidAt,
    DateTime? FailedAt,
    DateTime? CancelledAt,
    DateTime CreatedAt);

/// <summary>
/// Payment summary for list views — minimal info for receipts or booking details.
/// </summary>
public sealed record PaymentSummaryDto(
    Guid Id,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string Status,
    string Method,
    decimal RefundedAmount,
    DateTime? PaidAt,
    DateTime CreatedAt);

/// <summary>
/// Result DTO returned when a payment intent is created — includes client secret for frontend.
/// </summary>
public sealed record CreatePaymentResultDto(
    Guid PaymentId,
    string? ClientSecret,
    string? ProviderTransactionId,
    string Status);

/// <summary>
/// Result DTO returned after a refund is processed.
/// </summary>
public sealed record RefundResultDto(
    Guid PaymentId,
    decimal RefundedAmount,
    decimal TotalRefundedAmount,
    bool IsFullRefund,
    string Status);
