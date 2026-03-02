using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Identity.Application.Features.Register;

namespace StayHub.Services.Identity.Api.Controllers;

/// <summary>
/// Authentication controller — handles user registration, login, token refresh, and logout.
///
/// Endpoints:
/// POST /api/auth/register  — Create a new user account
/// POST /api/auth/login     — Authenticate and receive JWT tokens (commit 12)
/// POST /api/auth/refresh   — Refresh an expired access token (commit 12)
/// POST /api/auth/revoke    — Revoke a refresh token / logout (commit 12)
/// </summary>
[Route("api/auth")]
public sealed class AuthController : ApiController
{
    /// <summary>
    /// Register a new user account.
    /// </summary>
    /// <param name="request">Registration details (email, password, name, role).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 Created with user details, or 400/409 on failure.</returns>
    /// <response code="201">User created successfully.</response>
    /// <response code="400">Validation failed (invalid email, weak password, etc.).</response>
    /// <response code="409">Email already in use.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(request, cancellationToken);

        return HandleCreatedResult(result, nameof(Register), new { userId = result.IsSuccess ? result.Value.UserId : null });
    }
}
