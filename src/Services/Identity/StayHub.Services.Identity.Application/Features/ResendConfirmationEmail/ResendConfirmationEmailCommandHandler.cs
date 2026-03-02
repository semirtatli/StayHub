using Microsoft.Extensions.Logging;
using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Services.Identity.Application.IntegrationEvents;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Application.Features.ResendConfirmationEmail;

/// <summary>
/// Handles ResendConfirmationEmailCommand:
/// 1. Looks up the user by email
/// 2. Generates a new email confirmation token
/// 3. Publishes EmailVerificationRequestedEvent for the Notification Service
///
/// Intentionally does NOT return an error if the email is not found (security best practice:
/// prevents email enumeration attacks). Always returns success to the caller.
/// </summary>
internal sealed class ResendConfirmationEmailCommandHandler(
    IIdentityService identityService,
    IEmailVerificationSender emailVerificationSender,
    ILogger<ResendConfirmationEmailCommandHandler> logger)
    : ICommandHandler<ResendConfirmationEmailCommand>
{
    public async Task<Result> Handle(
        ResendConfirmationEmailCommand request,
        CancellationToken cancellationToken)
    {
        // Look up user — don't reveal if email exists or not
        var userResult = await identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (userResult.IsFailure)
        {
            logger.LogInformation(
                "Resend confirmation requested for unknown email {Email} — returning success to prevent enumeration",
                request.Email);
            return Result.Success();
        }

        var user = userResult.Value;

        // Already confirmed — silently succeed
        if (user.EmailConfirmed)
        {
            logger.LogInformation("Resend confirmation requested for already-confirmed email {Email}", request.Email);
            return Result.Success();
        }

        // Generate a new confirmation token
        var tokenResult = await identityService.GenerateEmailConfirmationTokenAsync(user.Id, cancellationToken);
        if (tokenResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to generate confirmation token for UserId {UserId}: {Error}",
                user.Id,
                tokenResult.Error.Code);
            return Result.Success(); // Still return success to prevent enumeration
        }

        // Publish to Notification Service (via integration event / in-process for now)
        await emailVerificationSender.SendVerificationEmailAsync(
            user.Id,
            user.Email,
            user.FirstName,
            tokenResult.Value,
            cancellationToken);

        logger.LogInformation("Email verification re-sent for UserId {UserId}", user.Id);

        return Result.Success();
    }
}
