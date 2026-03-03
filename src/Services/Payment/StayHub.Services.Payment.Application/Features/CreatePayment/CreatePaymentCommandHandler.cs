using Microsoft.Extensions.Logging;
using StayHub.Services.Payment.Application.Abstractions;
using StayHub.Services.Payment.Application.DTOs;
using StayHub.Services.Payment.Domain.Entities;
using StayHub.Services.Payment.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Payment.Application.Features.CreatePayment;

/// <summary>
/// Handles payment creation — creates an aggregate, calls the payment provider,
/// and marks the payment as Processing with the provider transaction ID.
///
/// TransactionBehavior commits the unit of work after a successful result.
/// </summary>
public sealed class CreatePaymentCommandHandler
    : ICommandHandler<CreatePaymentCommand, CreatePaymentResultDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentProvider _paymentProvider;
    private readonly ILogger<CreatePaymentCommandHandler> _logger;

    public CreatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentProvider paymentProvider,
        ILogger<CreatePaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _paymentProvider = paymentProvider;
        _logger = logger;
    }

    public async Task<Result<CreatePaymentResultDto>> Handle(
        CreatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        // Create the domain aggregate
        var payment = PaymentEntity.Create(
            request.BookingId,
            request.UserId,
            request.Amount,
            request.Currency,
            request.Method);

        // Call the external payment provider (e.g., Stripe)
        var metadata = new Dictionary<string, string>
        {
            ["bookingId"] = request.BookingId.ToString(),
            ["paymentId"] = payment.Id.ToString()
        };

        var providerResult = await _paymentProvider.CreatePaymentIntentAsync(
            request.Amount,
            request.Currency,
            request.Method,
            metadata,
            cancellationToken);

        if (!providerResult.IsSuccess)
        {
            payment.MarkAsFailed(providerResult.ErrorMessage ?? "Provider returned an error.");
            _paymentRepository.Add(payment);

            _logger.LogWarning(
                "Payment creation failed for booking {BookingId}: {Error}",
                request.BookingId,
                providerResult.ErrorMessage);

            return Result.Failure<CreatePaymentResultDto>(PaymentErrors.Payment.ProviderError);
        }

        // Mark as processing with the provider transaction ID
        payment.MarkAsProcessing(providerResult.ProviderTransactionId!, providerResult.ClientSecret);

        _paymentRepository.Add(payment);

        _logger.LogInformation(
            "Payment {PaymentId} created for booking {BookingId} — ProviderTxn: {ProviderTxnId}",
            payment.Id,
            request.BookingId,
            providerResult.ProviderTransactionId);

        return new CreatePaymentResultDto(
            payment.Id,
            providerResult.ClientSecret,
            providerResult.ProviderTransactionId,
            payment.Status.ToString());
    }
}
