using StayHub.Services.Payment.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Payment.Application.Features.GetPaymentById;

/// <summary>
/// Query to get a payment by its ID.
/// </summary>
public sealed record GetPaymentByIdQuery(
    Guid PaymentId,
    string UserId) : IQuery<PaymentDto>;
