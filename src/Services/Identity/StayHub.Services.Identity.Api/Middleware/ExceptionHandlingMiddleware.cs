using System.Net;
using System.Text.Json;
using StayHub.Shared.Exceptions;

namespace StayHub.Services.Identity.Api.Middleware;

/// <summary>
/// Global exception handler for the Identity Service API.
/// Maps domain exceptions to HTTP status codes with consistent JSON response.
///
/// Exception Mapping:
/// - NotFoundException → 404
/// - BusinessRuleException → 422
/// - ConflictException → 409
/// - ConcurrencyException → 409
/// - ForbiddenException → 403
/// - Unhandled → 500
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, message) = exception switch
        {
            NotFoundException nf => ((int)HttpStatusCode.NotFound, nf.ErrorCode, nf.Message),
            BusinessRuleException br => ((int)HttpStatusCode.UnprocessableEntity, br.ErrorCode, br.Message),
            ConflictException cf => ((int)HttpStatusCode.Conflict, cf.ErrorCode, cf.Message),
            ConcurrencyException cc => ((int)HttpStatusCode.Conflict, cc.ErrorCode, cc.Message),
            ForbiddenException fb => ((int)HttpStatusCode.Forbidden, fb.ErrorCode, fb.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.")
        };

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning("Domain exception for {Method} {Path}: {ErrorCode} - {Message}",
                context.Request.Method, context.Request.Path, errorCode, message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            status = statusCode,
            error = errorCode,
            message,
            correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
