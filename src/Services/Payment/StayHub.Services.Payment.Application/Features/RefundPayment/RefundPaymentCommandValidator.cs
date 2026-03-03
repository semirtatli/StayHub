using FluentValidation;

namespace StayHub.Services.Payment.Application.Features.RefundPayment;

/// <summary>
/// Validates RefundPaymentCommand before it reaches the handler.
/// </summary>
public sealed class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("Payment ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Refund amount must be greater than zero.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
