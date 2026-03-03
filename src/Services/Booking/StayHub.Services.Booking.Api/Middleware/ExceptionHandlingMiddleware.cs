using System.Text.Json;

namespace StayHub.Services.Booking.Api.Middleware;

/// <summary>
/// Global exception handling middleware for the Booking API.
/// Catches unhandled exceptions and returns consistent JSON error responses.
/// Maps domain exceptions to appropriate HTTP status codes.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, message) = exception switch
        {
            ArgumentException argEx => (StatusCodes.Status400BadRequest, "Validation.Error", argEx.Message),
            InvalidOperationException invEx => (StatusCodes.Status409Conflict, "Domain.Error", invEx.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Auth.Unauthorized", "You are not authorized to perform this action."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource.NotFound", "The requested resource was not found."),
            _ => (StatusCodes.Status500InternalServerError, "Server.Error", "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = statusCode,
            error = errorCode,
            message
        };

        var json = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}
