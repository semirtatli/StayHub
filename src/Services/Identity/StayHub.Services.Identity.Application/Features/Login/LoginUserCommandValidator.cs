using FluentValidation;

namespace StayHub.Services.Identity.Application.Features.Login;

/// <summary>
/// Validates LoginUserCommand — ensures email and password are provided.
/// Actual credential validation happens in the handler (against Identity).
/// </summary>
public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
