using Microsoft.Extensions.Logging;
using StayHub.Services.Notification.Application.Abstractions;

namespace StayHub.Services.Notification.Infrastructure.Email;

/// <summary>
/// Development email sender that logs email content instead of actually sending.
/// Replace with SmtpEmailSender or SendGridEmailSender for production.
/// </summary>
public sealed class LogEmailSender : IEmailSender
{
    private readonly ILogger<LogEmailSender> _logger;

    public LogEmailSender(ILogger<LogEmailSender> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendAsync(
        string recipient, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "========== EMAIL ==========\n" +
            "TO: {Recipient}\n" +
            "SUBJECT: {Subject}\n" +
            "BODY:\n{Body}\n" +
            "===========================",
            recipient, subject, htmlBody);

        return Task.FromResult(true);
    }
}
