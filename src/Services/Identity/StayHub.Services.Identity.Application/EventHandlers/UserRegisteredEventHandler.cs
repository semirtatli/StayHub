using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Services.Identity.Application.IntegrationEvents;
using StayHub.Services.Identity.Domain.Events;

namespace StayHub.Services.Identity.Application.EventHandlers;

/// <summary>
/// Handles UserRegisteredEvent by generating an email confirmation token
/// and dispatching a verification email through the IEmailVerificationSender.
///
/// This decouples registration from email sending — the RegisterUserCommandHandler
/// only creates the user; this handler takes care of the confirmation flow.
///
/// When the Notification Service is wired up via RabbitMQ/MassTransit,
/// the IEmailVerificationSender implementation will publish an integration event
/// instead of sending emails directly.
/// </summary>
internal sealed class UserRegisteredEventHandler(
    IIdentityService identityService,
    IEmailVerificationSender emailVerificationSender,
    ILogger<UserRegisteredEventHandler> logger) : INotificationHandler<UserRegisteredEvent>
{
    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling UserRegisteredEvent for UserId {UserId} — generating email confirmation token",
            notification.UserId);

        var tokenResult = await identityService.GenerateEmailConfirmationTokenAsync(
            notification.UserId,
            cancellationToken);

        if (tokenResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to generate email confirmation token for UserId {UserId}: {Error}",
                notification.UserId,
                tokenResult.Error.Code);
            return;
        }

        await emailVerificationSender.SendVerificationEmailAsync(
            notification.UserId,
            notification.Email,
            notification.FirstName,
            tokenResult.Value,
            cancellationToken);

        logger.LogInformation(
            "Email verification dispatched for UserId {UserId} ({Email})",
            notification.UserId,
            notification.Email);
    }
}
