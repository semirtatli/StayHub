using FluentValidation;

namespace StayHub.Services.Identity.Application.Features.ConfirmEmail;

/// <summary>
/// Validates ConfirmEmailCommand — both UserId and Token are required.
/// </summary>
public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Confirmation token is required.");
    }
}
