using Microsoft.Extensions.Logging;
using StayHub.Services.Payment.Application.Abstractions;
using StayHub.Services.Payment.Application.DTOs;
using StayHub.Services.Payment.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Payment.Application.Features.RefundPayment;

/// <summary>
/// Handles refund processing — calls the external provider, then updates the domain aggregate.
///
/// Validates that the payment exists, can be refunded, and the requested amount
/// does not exceed the refundable balance.
///
/// TransactionBehavior commits the unit of work after a successful result.
/// </summary>
public sealed class RefundPaymentCommandHandler
    : ICommandHandler<RefundPaymentCommand, RefundResultDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentProvider _paymentProvider;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;

    public RefundPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentProvider paymentProvider,
        ILogger<RefundPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _paymentProvider = paymentProvider;
        _logger = logger;
    }

    public async Task<Result<RefundResultDto>> Handle(
        RefundPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(
            request.PaymentId, cancellationToken);

        if (payment is null)
            return Result.Failure<RefundResultDto>(PaymentErrors.Payment.NotFound);

        if (!payment.CanRefund)
            return Result.Failure<RefundResultDto>(PaymentErrors.Payment.RefundNotAllowed);

        if (request.Amount > payment.RefundableAmount)
            return Result.Failure<RefundResultDto>(PaymentErrors.Payment.RefundExceedsAmount);

        // Call external provider to process the refund
        var providerResult = await _paymentProvider.RefundAsync(
            payment.ProviderTransactionId!,
            request.Amount,
            payment.Amount.Currency,
            cancellationToken);

        if (!providerResult.IsSuccess)
        {
            _logger.LogWarning(
                "Refund failed for payment {PaymentId}: {Error}",
                request.PaymentId,
                providerResult.ErrorMessage);

            return Result.Failure<RefundResultDto>(PaymentErrors.Payment.ProviderError);
        }

        // Update the domain aggregate
        try
        {
            payment.ProcessRefund(request.Amount);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure<RefundResultDto>(PaymentErrors.Payment.InvalidStatusTransition);
        }

        _paymentRepository.Update(payment);

        var isFullRefund = payment.RefundedAmount.Amount >= payment.Amount.Amount;

        _logger.LogInformation(
            "Refund of {Amount} {Currency} processed for payment {PaymentId} — Full: {IsFullRefund}",
            request.Amount,
            payment.Amount.Currency,
            payment.Id,
            isFullRefund);

        return new RefundResultDto(
            payment.Id,
            request.Amount,
            payment.RefundedAmount.Amount,
            isFullRefund,
            payment.Status.ToString());
    }
}
