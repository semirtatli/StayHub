using StayHub.Shared.CQRS;

namespace StayHub.Services.Payment.Application.Features.CancelPayment;

/// <summary>
/// Command to cancel a pending payment (e.g., booking cancelled before payment processing).
/// Transitions: Pending → Cancelled.
/// </summary>
public sealed record CancelPaymentCommand(
    Guid PaymentId,
    string UserId) : ICommand;
