using StayHub.Services.Payment.Application.DTOs;
using StayHub.Services.Payment.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Payment.Application.Features.GetPaymentById;

/// <summary>
/// Handles getting a payment by ID — returns full payment details.
/// </summary>
public sealed class GetPaymentByIdQueryHandler : IQueryHandler<GetPaymentByIdQuery, PaymentDto>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentByIdQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<PaymentDto>> Handle(
        GetPaymentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(
            request.PaymentId, cancellationToken);

        if (payment is null)
            return Result.Failure<PaymentDto>(PaymentErrors.Payment.NotFound);

        // Verify ownership — only the payment owner can view details
        if (payment.UserId != request.UserId)
            return Result.Failure<PaymentDto>(PaymentErrors.Payment.NotOwner);

        return payment.ToDto();
    }
}
