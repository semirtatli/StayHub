using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Identity.Application.Features.AssignRole;
using StayHub.Services.Identity.Application.Features.GetUser;

namespace StayHub.Services.Identity.Api.Controllers;

/// <summary>
/// User management controller — profile access and admin role management.
///
/// Endpoints:
/// GET  /api/users/me          — Get current user's profile (any authenticated)
/// GET  /api/users/{id}        — Get user by ID (Admin only)
/// POST /api/users/{id}/role   — Assign role to user (Admin only)
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
