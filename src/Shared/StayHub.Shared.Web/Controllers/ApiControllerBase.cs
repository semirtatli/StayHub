using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ApiEnvelope = StayHub.Shared.Web.Response.ApiResponse;
using StayHub.Shared.Web.Response;

namespace StayHub.Shared.Web.Controllers;

/// <summary>
/// Base controller for all StayHub API controllers.
/// Provides consistent Result-to-HTTP mapping using ApiResponse envelope.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult(Shared.Result.Result result)
    {
        if (result.IsSuccess)
            return Ok(ApiEnvelope.Ok());

        return HandleFailure(result);
    }

    protected IActionResult HandleResult<T>(Shared.Result.Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<T>.Ok(result.Value));

        return HandleFailure(result);
    }

    protected IActionResult HandleResult<T>(Shared.Result.Result<T> result, PaginationMeta meta)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<T>.Ok(result.Value, meta));

        return HandleFailure(result);
    }

    protected IActionResult HandleCreatedResult<T>(Shared.Result.Result<T> result, string actionName, object? routeValues = null)
    {
        if (result.IsSuccess)
            return CreatedAtAction(actionName, routeValues, ApiResponse<T>.Ok(result.Value));

        return HandleFailure(result);
    }

    private IActionResult HandleFailure(Shared.Result.Result result)
    {
        if (result is Shared.Result.IValidationResult validationResult)
        {
            var errors = validationResult.Errors
                .Select(e => new ApiError(e.Code, e.Message));

            return BadRequest(ApiEnvelope.Fail(errors));
        }

        var statusCode = result.Error.Code switch
        {
            var c when c.Contains("NotFound") => StatusCodes.Status404NotFound,
            var c when c.Contains("Duplicate") || c.Contains("Conflict") || c.Contains("AlreadyReviewed") => StatusCodes.Status409Conflict,
            var c when c.Contains("Unauthorized") || c.Contains("InvalidCredentials") || c.Contains("InvalidWebhookSignature") => StatusCodes.Status401Unauthorized,
            var c when c.Contains("Forbidden") || c.Contains("NotOwner") || c.Contains("NotGuest") || c.Contains("NotHotelOwner") || c.Contains("NotAuthor") => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        return StatusCode(statusCode, ApiEnvelope.Fail(result.Error.Code, result.Error.Message));
    }
}
