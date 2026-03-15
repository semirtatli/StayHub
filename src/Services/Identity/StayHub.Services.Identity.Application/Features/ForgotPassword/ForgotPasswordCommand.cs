using StayHub.Shared.CQRS;

namespace StayHub.Services.Identity.Application.Features.ForgotPassword;

/// <summary>
/// Command to request a password reset token.
/// Always returns success to prevent email enumeration attacks.
/// </summary>
public sealed record ForgotPasswordCommand(string Email) : ICommand;
