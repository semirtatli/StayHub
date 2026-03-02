using MediatR;
using Microsoft.AspNetCore.Mvc;
using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Api.Controllers;

/// <summary>
/// Base controller with shared helpers for mapping Result pattern to HTTP responses.
///
/// All Identity API controllers inherit from this to get consistent:
/// - MediatR access via Mediator property
/// - Result → HTTP status code mapping
/// - Validation error formatting
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    private ISender? _mediator;

    /// <summary>
    /// Lazy-resolved MediatR sender — avoids constructor injection in every derived controller.
    /// </summary>
    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Maps a non-generic Result to an HTTP response.
    /// Success → 200 OK, Failure → appropriate error status code.
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
    /// Success → 200 OK with value, Failure → appropriate error status code.
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
    /// Maps a generic Result&lt;T&gt; to 201 Created with a location header.
    /// </summary>
    protected IActionResult HandleCreatedResult<T>(Result<T> result, string actionName, object? routeValues = null)
    {
        if (result.IsSuccess)
        {
            return CreatedAtAction(actionName, routeValues, result.Value);
        }

        return HandleFailure(result);
    }

    /// <summary>
    /// Maps a failed Result to the appropriate HTTP error response.
    ///
    /// Error code mapping:
    /// - Validation errors → 400 Bad Request (with list of errors)
    /// - NotFound → 404
    /// - Duplicate/Conflict → 409
    /// - Unauthorized → 401
    /// - Forbidden → 403
    /// - Everything else → 400
    /// </summary>
    private IActionResult HandleFailure(Result result)
    {
        // Validation errors come as ValidationResult with multiple errors
        if (result is IValidationResult validationResult)
        {
            return BadRequest(new
            {
                status = 400,
                error = "VALIDATION_ERROR",
                message = "One or more validation errors occurred.",
                errors = validationResult.Errors.Select(e => new { field = e.Code, message = e.Message })
            });
        }

        return result.Error.Code switch
        {
            _ when result.Error.Code.Contains("NotFound") => NotFound(new
            {
                status = 404,
                error = result.Error.Code,
                message = result.Error.Message
            }),
            _ when result.Error.Code.Contains("Duplicate") || result.Error.Code.Contains("Conflict") => Conflict(new
            {
                status = 409,
                error = result.Error.Code,
                message = result.Error.Message
            }),
            _ when result.Error.Code.Contains("Unauthorized") || result.Error.Code.Contains("InvalidCredentials") => Unauthorized(new
            {
                status = 401,
                error = result.Error.Code,
                message = result.Error.Message
            }),
            _ when result.Error.Code.Contains("Forbidden") => StatusCode(403, new
            {
                status = 403,
                error = result.Error.Code,
                message = result.Error.Message
            }),
            _ => BadRequest(new
            {
                status = 400,
                error = result.Error.Code,
                message = result.Error.Message
            })
        };
    }
}
