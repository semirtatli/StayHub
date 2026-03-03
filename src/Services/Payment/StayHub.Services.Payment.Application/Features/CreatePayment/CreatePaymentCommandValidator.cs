using FluentValidation;

namespace StayHub.Services.Payment.Application.Features.CreatePayment;

/// <summary>
/// Validates CreatePaymentCommand before it reaches the handler.
/// Runs automatically in the ValidationBehavior pipeline.
/// </summary>
public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("Booking ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code (e.g., USD).");

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage("Invalid payment method.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
