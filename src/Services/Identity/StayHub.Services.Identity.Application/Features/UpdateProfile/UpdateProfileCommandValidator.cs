using FluentValidation;

namespace StayHub.Services.Identity.Application.Features.UpdateProfile;

/// <summary>
/// Validates UpdateProfileCommand — ensures names are valid and phone is properly formatted.
/// </summary>
public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.")
            .Matches(@"^[\p{L}\s\-']+$").WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.")
            .Matches(@"^[\p{L}\s\-']+$").WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number must be in E.164 format (e.g., +905551234567).");
    }
}
