using FluentValidation;

namespace StayHub.Services.Identity.Application.Features.RevokeToken;

/// <summary>
/// Validates RevokeTokenCommand — token must be present.
/// </summary>
public sealed class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
