using StayHub.Shared.CQRS;

namespace StayHub.Services.Identity.Application.Features.ResendConfirmationEmail;

/// <summary>
/// Command to resend the email confirmation link.
/// Generates a new confirmation token and publishes an integration event
/// for the Notification Service to send the email.
/// </summary>
public sealed record ResendConfirmationEmailCommand(string Email) : ICommand;
