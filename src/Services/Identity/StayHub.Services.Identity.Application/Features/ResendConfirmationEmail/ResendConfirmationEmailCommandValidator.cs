using FluentValidation;

namespace StayHub.Services.Identity.Application.Features.ResendConfirmationEmail;

/// <summary>
/// Validates ResendConfirmationEmailCommand — email format check.
/// </summary>
public sealed class ResendConfirmationEmailCommandValidator
    : AbstractValidator<ResendConfirmationEmailCommand>
{
    public ResendConfirmationEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
