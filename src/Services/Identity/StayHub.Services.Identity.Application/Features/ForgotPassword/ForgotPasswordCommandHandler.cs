using Microsoft.Extensions.Logging;
using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Application.Features.ForgotPassword;

/// <summary>
/// Handles ForgotPasswordCommand:
/// 1. Looks up the user by email
/// 2. Generates a password reset token via UserManager
/// 3. Logs the token in development (no actual email sending)
///
/// Intentionally does NOT return an error if the email is not found (security best practice:
/// prevents email enumeration attacks). Always returns success to the caller.
/// </summary>
internal sealed class ForgotPasswordCommandHandler(
    IIdentityService identityService,
    ILogger<ForgotPasswordCommandHandler> logger)
    : ICommandHandler<ForgotPasswordCommand>
{
    public async Task<Result> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // Look up user — don't reveal if email exists or not
        var userResult = await identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (userResult.IsFailure)
        {
            logger.LogInformation(
                "Password reset requested for unknown email {Email} — returning success to prevent enumeration",
                request.Email);
            return Result.Success();
        }

        var user = userResult.Value;

        // Generate a password reset token
        var tokenResult = await identityService.GeneratePasswordResetTokenAsync(user.Id, cancellationToken);
        if (tokenResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to generate password reset token for UserId {UserId}: {Error}",
                user.Id,
                tokenResult.Error.Code);
            return Result.Success(); // Still return success to prevent enumeration
        }

        // In development, log the token. In production, this would send an email.
        logger.LogInformation(
            "Password reset token for {Email}: {Token}",
            request.Email,
            tokenResult.Value);

        return Result.Success();
    }
}
