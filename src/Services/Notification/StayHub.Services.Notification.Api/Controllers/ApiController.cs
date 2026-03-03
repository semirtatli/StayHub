using MediatR;
using Microsoft.AspNetCore.Mvc;
using StayHub.Shared.Result;

namespace StayHub.Services.Notification.Api.Controllers;

/// <summary>
/// Base controller with shared helpers for mapping Result pattern to HTTP responses.
/// All Notification API controllers inherit from this for consistent error handling.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    private ISender? _mediator;

    /// <summary>
    /// Lazy-resolved MediatR sender.
    /// </summary>
    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Maps a non-generic Result to an HTTP response.
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return HandleFailure(result);
    }

    /// <summary>
    /// Maps a generic Result&lt;T&gt; to an HTTP response.
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return HandleFailure(result);
    }

    /// <summary>
    /// Maps a failed Result to the appropriate HTTP error response.
    /// </summary>
    private IActionResult HandleFailure(Result result)
    {
        if (result is IValidationResult validationResult)
        {
            return BadRequest(new
            {
                status = StatusCodes.Status400BadRequest,
                error = "Validation.Error",
                errors = validationResult.Errors
                    .Select(e => new { code = e.Code, message = e.Message })
            });
        }

        var statusCode = result.Error.Code switch
        {
            var code when code.Contains("NotFound") => StatusCodes.Status404NotFound,
            var code when code.Contains("Duplicate") || code.Contains("Conflict") => StatusCodes.Status409Conflict,
            var code when code.Contains("Unauthorized") => StatusCodes.Status401Unauthorized,
            var code when code.Contains("Forbidden") => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        return StatusCode(statusCode, new
        {
            status = statusCode,
            error = result.Error.Code,
            message = result.Error.Message
        });
    }
}
