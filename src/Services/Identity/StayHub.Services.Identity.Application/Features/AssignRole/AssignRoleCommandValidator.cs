using FluentValidation;
using StayHub.Services.Identity.Domain.Enums;

namespace StayHub.Services.Identity.Application.Features.AssignRole;

/// <summary>
/// Validates AssignRoleCommand — ensures userId and role are valid.
/// Authorization (Admin-only) is handled by the controller's [Authorize] attribute.
/// </summary>
public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => AppRoles.All.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", AppRoles.All)}.");

        RuleFor(x => x.AssignedByUserId)
            .NotEmpty().WithMessage("Assigner user ID is required.");
    }
}
