using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Identity.Application.Features.AssignRole;
using StayHub.Services.Identity.Application.Features.ChangePassword;
using StayHub.Services.Identity.Application.Features.GetUser;
using StayHub.Services.Identity.Application.Features.UpdateProfile;

namespace StayHub.Services.Identity.Api.Controllers;

/// <summary>
/// User management controller — profile access, profile updates, and admin role management.
///
/// Endpoints:
/// GET  /api/users/me              — Get current user's profile (any authenticated)
/// PUT  /api/users/me/profile      — Update current user's profile (any authenticated)
/// PUT  /api/users/me/password     — Change current user's password (any authenticated)
/// GET  /api/users/{id}            — Get user by ID (Admin only)
/// POST /api/users/{id}/role       — Assign role to user (Admin only)
/// </summary>
[Route("api/users")]
public sealed class UsersController : ApiController
{
    /// <summary>
    /// Get the current authenticated user's profile.
    /// </summary>
    [HttpGet("me")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var query = new GetUserByIdQuery(userId);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Update the current authenticated user's profile (name, phone number).
    /// </summary>
    [HttpPut("me/profile")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var command = new UpdateProfileCommand(
            userId,
            request.FirstName,
            request.LastName,
            request.PhoneNumber);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Change the current authenticated user's password.
    /// Revokes all existing refresh tokens as a security measure.
    /// </summary>
    [HttpPut("me/password")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var command = new ChangePasswordCommand(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmNewPassword);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Get a user by ID. Admin only.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(
        string id,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Assign a role to a user. Admin only.
    /// </summary>
    [HttpPost("{id}/role")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(
        string id,
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        var assignedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

        var command = new AssignRoleCommand(id, request.Role, assignedByUserId);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}

/// <summary>
/// Request body for role assignment.
/// </summary>
public sealed record AssignRoleRequest(string Role);

/// <summary>
/// Request body for profile update.
/// </summary>
public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber);

/// <summary>
/// Request body for password change.
/// </summary>
public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);
