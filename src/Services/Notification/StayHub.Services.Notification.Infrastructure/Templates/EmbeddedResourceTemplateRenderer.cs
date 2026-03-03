using System.Reflection;
using Microsoft.Extensions.Logging;
using StayHub.Services.Notification.Application.Abstractions;

namespace StayHub.Services.Notification.Infrastructure.Templates;

/// <summary>
/// Simple template renderer that loads embedded HTML resource templates
/// and performs {{placeholder}} string replacement.
///
/// Templates are embedded resources in the Templates/ folder of this assembly.
/// Each template file is named: {TemplateName}.html
///
/// Replacement syntax: {{Key}} is replaced with the corresponding value
/// from the model dictionary. Unmatched placeholders remain unchanged.
///
/// For production, consider upgrading to Razor/Scriban for more powerful templating.
/// </summary>
public sealed class EmbeddedResourceTemplateRenderer : ITemplateRenderer
{
    private readonly ILogger<EmbeddedResourceTemplateRenderer> _logger;
    private readonly Assembly _assembly;
    private readonly string _resourcePrefix;

    public EmbeddedResourceTemplateRenderer(ILogger<EmbeddedResourceTemplateRenderer> logger)
    {
        _logger = logger;
        _assembly = typeof(EmbeddedResourceTemplateRenderer).Assembly;
        _resourcePrefix = "StayHub.Services.Notification.Infrastructure.Templates.";
    }

    public async Task<string> RenderAsync(
        string templateName,
        Dictionary<string, string> model,
        CancellationToken cancellationToken = default)
    {
        var resourceName = $"{_resourcePrefix}{templateName}.html";

        await using var stream = _assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            _logger.LogWarning(
                "Template {TemplateName} not found as embedded resource {ResourceName}. Using fallback.",
                templateName, resourceName);

            // Fallback: generate a simple HTML from the model data
            return GenerateFallbackHtml(templateName, model);
        }

        using var reader = new StreamReader(stream);
        var template = await reader.ReadToEndAsync(cancellationToken);

        // Replace {{Key}} placeholders with model values
        foreach (var (key, value) in model)
        {
            template = template.Replace($"{{{{{key}}}}}", value);
        }

        return template;
    }

    private static string GenerateFallbackHtml(string templateName, Dictionary<string, string> model)
    {
        var rows = string.Join("\n",
            model.Select(kvp => $"<tr><td><strong>{kvp.Key}</strong></td><td>{kvp.Value}</td></tr>"));

        return $"""
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8" /><title>{templateName}</title></head>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
                <h2 style="color: #2563eb;">StayHub — {templateName}</h2>
                <table style="width: 100%; border-collapse: collapse;">
                    {rows}
                </table>
                <hr />
                <p style="color: #6b7280; font-size: 12px;">
                    This is an automated notification from StayHub. Please do not reply.
                </p>
            </body>
            </html>
            """;
    }
}
