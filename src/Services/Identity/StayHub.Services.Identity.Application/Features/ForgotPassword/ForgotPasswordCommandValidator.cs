using FluentValidation;

namespace StayHub.Services.Identity.Application.Features.ForgotPassword;

/// <summary>
/// Validates ForgotPasswordCommand — email format check.
/// </summary>
public sealed class ForgotPasswordCommandValidator
    : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
