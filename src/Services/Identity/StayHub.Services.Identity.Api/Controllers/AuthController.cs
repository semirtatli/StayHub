using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Services.Identity.Application.Features.ConfirmEmail;
using StayHub.Services.Identity.Application.Features.Login;
using StayHub.Services.Identity.Application.Features.RefreshToken;
using StayHub.Services.Identity.Application.Features.Register;
using StayHub.Services.Identity.Application.Features.ResendConfirmationEmail;
using StayHub.Services.Identity.Application.Features.RevokeToken;

namespace StayHub.Services.Identity.Api.Controllers;

/// <summary>
/// Authentication controller — handles user registration, login, token refresh,
/// logout, and email verification.
///
/// Security design:
/// - Access token (JWT, 15min) → returned in response body, stored in memory by client
/// - Refresh token (random, 7 days) → sent as httpOnly secure cookie (prevents XSS access)
/// - Token rotation on every refresh (prevents replay attacks)
/// - Revoked token reuse detection (revokes entire token family)
/// - Email confirmation required for full account access
/// </summary>
[Route("api/auth")]
public sealed class AuthController : ApiController
{
    private const string RefreshTokenCookieName = "refreshToken";

    /// <summary>
    /// Register a new user account.
    /// </summary>
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

    /// <summary>
    /// Authenticate with email and password.
    /// Returns access token in body; sets refresh token as httpOnly cookie.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var ipAddress = GetIpAddress();

        var command = new LoginUserCommand(request.Email, request.Password, ipAddress);
        var result = await Mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleResult(result);
        }

        SetRefreshTokenCookie(result.Value.RefreshToken);

        return Ok(new LoginResponse(
            result.Value.AccessToken,
            result.Value.AccessTokenExpiresAt,
            result.Value.User));
    }

    /// <summary>
    /// Refresh an expired access token using the refresh token cookie.
    /// Issues a new token pair (rotation).
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new { status = 401, error = "Token.Missing", message = "Refresh token cookie not found." });
        }

        var ipAddress = GetIpAddress();
        var command = new RefreshTokenCommand(refreshToken, ipAddress);
        var result = await Mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            // Clear the invalid cookie
            DeleteRefreshTokenCookie();
            return HandleResult(result);
        }

        SetRefreshTokenCookie(result.Value.RefreshToken);

        return Ok(new LoginResponse(
            result.Value.AccessToken,
            result.Value.AccessTokenExpiresAt,
            result.Value.User));
    }

    /// <summary>
    /// Revoke the current refresh token (logout).
    /// Clears the refresh token cookie.
    /// </summary>
    [HttpPost("revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Revoke(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return BadRequest(new { status = 400, error = "Token.Missing", message = "Refresh token cookie not found." });
        }

        var ipAddress = GetIpAddress();
        var command = new RevokeTokenCommand(refreshToken, ipAddress);
        var result = await Mediator.Send(command, cancellationToken);

        DeleteRefreshTokenCookie();

        return HandleResult(result);
    }

    /// <summary>
    /// Confirm a user's email address using the confirmation token.
    /// Called when the user clicks the link in their verification email.
    /// </summary>
    [HttpGet("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string userId,
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var command = new ConfirmEmailCommand(userId, token);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Resend the email confirmation link.
    /// Always returns 200 OK regardless of whether the email exists (prevents email enumeration).
    /// </summary>
    [HttpPost("resend-confirmation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendConfirmation(
        [FromBody] ResendConfirmationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResendConfirmationEmailCommand(request.Email);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    // ── Private helpers ──────────────────────────────────────────────────

    /// <summary>
    /// Sets the refresh token as an httpOnly secure cookie.
    /// - HttpOnly: prevents JavaScript access (XSS protection)
    /// - Secure: only sent over HTTPS
    /// - SameSite=Strict: prevents CSRF
    /// - Path=/api/auth: only sent to auth endpoints
    /// </summary>
    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/api/auth"
        };

        Response.Cookies.Append(RefreshTokenCookieName, token, cookieOptions);
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Path = "/api/auth"
        });
    }

    private string GetIpAddress()
    {
        // Check for forwarded IP (behind reverse proxy / API gateway)
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(ip))
            {
                // X-Forwarded-For can be comma-separated; take the first (client) IP
                return ip.Split(',')[0].Trim();
            }
        }

        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
    }
}

// ── Request/Response DTOs for the API layer ──────────────────────────────

/// <summary>
/// Login request body — only email and password.
/// IP address is extracted from the request headers by the controller.
/// </summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// Login/refresh response — access token + user info.
/// Refresh token is NOT in the body — it's in the httpOnly cookie.
/// </summary>
public sealed record LoginResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    UserDto User);

/// <summary>
/// Request body for resending email confirmation.
/// </summary>
public sealed record ResendConfirmationRequest(string Email);
