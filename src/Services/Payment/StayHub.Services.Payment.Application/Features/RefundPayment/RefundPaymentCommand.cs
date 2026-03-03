using StayHub.Services.Payment.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Payment.Application.Features.RefundPayment;

/// <summary>
/// Command to process a refund for a succeeded payment.
///
/// Supports both full and partial refunds. The refund amount is validated
/// against the remaining refundable balance in the domain entity.
///
/// UserId is set by the controller from the authenticated user's JWT claims.
/// </summary>
public sealed record RefundPaymentCommand(
    Guid PaymentId,
    decimal Amount,
    string UserId) : ICommand<RefundResultDto>;
