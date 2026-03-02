using FluentValidation;

namespace StayHub.Services.Identity.Application.Features.RefreshToken;

/// <summary>
/// Validates RefreshTokenCommand — token string must be present.
/// </summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
