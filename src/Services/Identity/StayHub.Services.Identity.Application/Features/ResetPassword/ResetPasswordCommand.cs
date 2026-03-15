using StayHub.Shared.CQRS;

namespace StayHub.Services.Identity.Application.Features.ResetPassword;

/// <summary>
/// Command to reset a user's password using a previously issued reset token.
/// </summary>
public sealed record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword) : ICommand;
