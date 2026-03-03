using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace StayHub.Shared.Web.Middleware;

/// <summary>
/// Propagates X-Correlation-Id from the API Gateway to the current request scope
/// and enriches the Serilog LogContext for structured logging.
/// If no correlation ID header is present, a new GUID is generated.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString("D");

        // Make available to downstream services via response header
        context.Response.Headers[HeaderName] = correlationId;

        // Store in HttpContext.Items for easy access in handlers
        context.Items["CorrelationId"] = correlationId;

        // Enrich Serilog log context so every log line includes CorrelationId
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
