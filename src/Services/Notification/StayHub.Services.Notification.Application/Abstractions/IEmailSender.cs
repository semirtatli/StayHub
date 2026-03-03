namespace StayHub.Services.Notification.Application.Abstractions;

/// <summary>
/// Abstraction for sending emails.
///
/// Implementations:
/// - LogEmailSender: Logs email content (development)
/// - SmtpEmailSender: Real SMTP delivery (production - future)
/// - SendGridEmailSender: SendGrid API delivery (production - future)
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email to the specified recipient.
    /// </summary>
    /// <param name="recipient">Recipient email address.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="htmlBody">Rendered HTML body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if delivery succeeded; false otherwise.</returns>
    Task<bool> SendAsync(
        string recipient,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
