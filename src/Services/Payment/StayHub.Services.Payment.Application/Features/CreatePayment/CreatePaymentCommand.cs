using StayHub.Services.Payment.Application.DTOs;
using StayHub.Services.Payment.Domain.Enums;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Payment.Application.Features.CreatePayment;

/// <summary>
/// Command to initiate a payment for a booking.
///
/// Flow: Controller → Validation → Handler → IPaymentProvider.CreatePaymentIntentAsync
///       → creates PaymentEntity aggregate → marks as Processing → persisted via TransactionBehavior.
///
/// UserId is set by the controller from the authenticated user's JWT claims.
/// </summary>
public sealed record CreatePaymentCommand(
    Guid BookingId,
    decimal Amount,
    string Currency,
    PaymentMethod Method,
    string UserId) : ICommand<CreatePaymentResultDto>;
