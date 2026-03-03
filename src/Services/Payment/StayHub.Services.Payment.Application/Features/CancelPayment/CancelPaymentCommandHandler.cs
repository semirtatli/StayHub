using Microsoft.Extensions.Logging;
using StayHub.Services.Payment.Application.Abstractions;
using StayHub.Services.Payment.Domain.Enums;
using StayHub.Services.Payment.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Payment.Application.Features.CancelPayment;

/// <summary>
/// Handles payment cancellation — cancels with the provider if processing,
/// then cancels the domain aggregate.
///
/// TransactionBehavior commits the unit of work after a successful result.
/// </summary>
public sealed class CancelPaymentCommandHandler : ICommandHandler<CancelPaymentCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentProvider _paymentProvider;
    private readonly ILogger<CancelPaymentCommandHandler> _logger;

    public CancelPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentProvider paymentProvider,
        ILogger<CancelPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _paymentProvider = paymentProvider;
        _logger = logger;
    }

    public async Task<Result> Handle(
        CancelPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(
            request.PaymentId, cancellationToken);

        if (payment is null)
            return Result.Failure(PaymentErrors.Payment.NotFound);

        if (payment.Status != PaymentStatus.Pending)
            return Result.Failure(PaymentErrors.Payment.InvalidStatusTransition);

        // If the provider has a transaction ID, cancel it with the provider
        if (!string.IsNullOrWhiteSpace(payment.ProviderTransactionId))
        {
            await _paymentProvider.CancelPaymentIntentAsync(
                payment.ProviderTransactionId, cancellationToken);
        }

        try
        {
            payment.Cancel();
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(PaymentErrors.Payment.InvalidStatusTransition);
        }

        _paymentRepository.Update(payment);

        _logger.LogInformation(
            "Payment {PaymentId} cancelled for booking {BookingId}",
            payment.Id, payment.BookingId);

        return Result.Success();
    }
}
