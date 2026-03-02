using System.Net;
using System.Text.Json;

namespace StayHub.ApiGateway.Middleware;

/// <summary>
/// Global exception handler for the API Gateway.
/// Catches unhandled exceptions and returns a consistent JSON error response
/// instead of leaking stack traces or HTML error pages.
///
/// This only handles gateway-level exceptions (e.g., YARP proxy failures,
/// configuration errors). Backend service errors are proxied as-is.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled gateway exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadGateway;

        var response = new
        {
            status = context.Response.StatusCode,
            error = "Gateway Error",
            message = "An error occurred while processing your request through the gateway.",
            correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
        };

        var json = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}
