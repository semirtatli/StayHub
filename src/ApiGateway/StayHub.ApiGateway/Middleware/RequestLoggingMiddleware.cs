using System.Diagnostics;
using System.Net;

namespace StayHub.ApiGateway.Middleware;

/// <summary>
/// Middleware that logs every proxied request with timing, status, and route info.
/// Sits at the top of the middleware pipeline to capture the full request lifecycle.
///
/// Logs: HTTP method, path, status code, elapsed time, client IP, correlation ID.
/// Structured logging fields are searchable in Seq.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate correlation ID for distributed tracing across services
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Request.Headers["X-Correlation-Id"] = correlationId;
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
            stopwatch.Stop();

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms [CorrelationId: {CorrelationId}] [ClientIP: {ClientIp}]",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId,
                GetClientIp(context));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "HTTP {Method} {Path} failed after {ElapsedMs}ms [CorrelationId: {CorrelationId}] [ClientIP: {ClientIp}]",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                correlationId,
                GetClientIp(context));

            throw;
        }
    }

    private static string GetClientIp(HttpContext context)
    {
        // Check X-Forwarded-For first (when behind a load balancer)
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
