using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StayHub.Services.Identity.Application.IntegrationEvents;

namespace StayHub.Services.Identity.Infrastructure.Email;

/// <summary>
/// Development implementation of IEmailVerificationSender.
/// Logs the confirmation link instead of sending an actual email.
///
/// In production, this will be replaced by an implementation that publishes
/// an integration event to RabbitMQ via MassTransit for the Notification Service
/// to consume and send the actual email (SendGrid, SMTP, etc.).
///
/// The confirmation URL follows the pattern:
///   {BaseUrl}/api/auth/confirm-email?userId={userId}&amp;token={urlEncodedToken}
/// In a real app, this would typically be a frontend URL that calls the API.
/// </summary>
public sealed class LogEmailVerificationSender(
    IConfiguration configuration,
    ILogger<LogEmailVerificationSender> logger) : IEmailVerificationSender
{
    public Task SendVerificationEmailAsync(
        string userId,
        string email,
        string firstName,
        string confirmationToken,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["App:BaseUrl"] ?? "https://localhost:5101";
        var encodedToken = Uri.EscapeDataString(confirmationToken);
        var confirmationUrl = $"{baseUrl}/api/auth/confirm-email?userId={userId}&token={encodedToken}";

        // In development, log the full link so developers can test without an email service
        logger.LogInformation(
            """
            ╔══════════════════════════════════════════════════════════════╗
            ║                  EMAIL VERIFICATION                         ║
            ╠══════════════════════════════════════════════════════════════╣
            ║ To: {Email}
            ║ Name: {FirstName}
            ║ UserId: {UserId}
            ║ Confirmation URL:
            ║ {ConfirmationUrl}
            ╚══════════════════════════════════════════════════════════════╝
            """,
            email,
            firstName,
            userId,
            confirmationUrl);

        return Task.CompletedTask;
    }
}
