using StayHub.Shared.CQRS;

namespace StayHub.Services.Identity.Application.Features.AssignRole;

/// <summary>
/// Command to assign a role to a user. Admin-only operation.
///
/// Business rules:
/// - Only Admin users can assign roles
/// - Cannot assign a role the user already has
/// - Target user must exist and be active
/// - Publishes UserRoleChangedEvent for downstream handlers
/// </summary>
public sealed record AssignRoleCommand(
    string UserId,
    string Role,
    string AssignedByUserId) : ICommand;
