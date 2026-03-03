namespace StayHub.Services.Notification.Application.Abstractions;

/// <summary>
/// Abstraction for rendering email templates with model data.
///
/// Implementations:
/// - RazorTemplateRenderer: Uses Razor engine for HTML templates
/// - SimpleTemplateRenderer: String replacement for basic templates (fallback)
/// </summary>
public interface ITemplateRenderer
{
    /// <summary>
    /// Renders an email template with the provided model data.
    /// </summary>
    /// <param name="templateName">Name of the template (e.g., "BookingConfirmation").</param>
    /// <param name="model">Dictionary of key-value pairs to inject into the template.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Rendered HTML string.</returns>
    Task<string> RenderAsync(
        string templateName,
        Dictionary<string, string> model,
        CancellationToken cancellationToken = default);
}
