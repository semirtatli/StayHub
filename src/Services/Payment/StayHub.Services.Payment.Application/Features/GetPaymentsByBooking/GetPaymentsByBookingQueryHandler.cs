using StayHub.Services.Payment.Application.DTOs;
using StayHub.Services.Payment.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Payment.Application.Features.GetPaymentsByBooking;

/// <summary>
/// Handles getting all payments for a booking — returns summary list.
/// </summary>
public sealed class GetPaymentsByBookingQueryHandler
    : IQueryHandler<GetPaymentsByBookingQuery, IReadOnlyList<PaymentSummaryDto>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentsByBookingQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<IReadOnlyList<PaymentSummaryDto>>> Handle(
        GetPaymentsByBookingQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetByBookingIdAsync(
            request.BookingId, cancellationToken);

        if (payments.Count == 0)
            return Result.Success<IReadOnlyList<PaymentSummaryDto>>(
                Array.Empty<PaymentSummaryDto>());

        // Verify ownership — all payments for a booking belong to the same user
        if (payments[0].UserId != request.UserId)
            return Result.Failure<IReadOnlyList<PaymentSummaryDto>>(
                PaymentErrors.Payment.NotOwner);

        var dtos = payments.Select(p => p.ToSummaryDto()).ToList();

        return Result.Success<IReadOnlyList<PaymentSummaryDto>>(dtos);
    }
}
