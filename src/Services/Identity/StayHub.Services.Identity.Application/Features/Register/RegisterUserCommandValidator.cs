using FluentValidation;
using StayHub.Services.Identity.Domain.Enums;

namespace StayHub.Services.Identity.Application.Features.Register;

/// <summary>
/// Validates RegisterUserCommand before it reaches the handler.
/// Runs automatically in the ValidationBehavior pipeline.
///
/// Rules:
/// - Email: required, valid format, max 256 chars
/// - Password: required, min 8 chars, must contain upper/lower/digit/special
/// - ConfirmPassword: must match Password
/// - FirstName/LastName: required, 2-100 chars, letters/spaces/hyphens only
/// - Role: must be a valid AppRole (Guest or HotelOwner — Admin is assigned separately)
/// </summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Password confirmation is required.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");

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

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => role == AppRoles.Guest || role == AppRoles.HotelOwner)
            .WithMessage($"Role must be either '{AppRoles.Guest}' or '{AppRoles.HotelOwner}'. Admin role is assigned by administrators only.");
    }
}
