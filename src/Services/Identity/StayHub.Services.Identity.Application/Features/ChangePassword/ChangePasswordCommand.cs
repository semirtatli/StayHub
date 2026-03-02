using StayHub.Shared.CQRS;

namespace StayHub.Services.Identity.Application.Features.ChangePassword;

/// <summary>
/// Command to change the authenticated user's password.
/// Requires the current password for verification.
/// On success, all refresh tokens are revoked (security measure).
/// </summary>
public sealed record ChangePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword) : ICommand;
