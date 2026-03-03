using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Notification.Application.Features.GetNotificationById;
using StayHub.Services.Notification.Application.Features.GetUserNotifications;
using StayHub.Services.Notification.Application.Features.RetryFailedNotifications;

namespace StayHub.Services.Notification.Api.Controllers;

/// <summary>
/// Notification management endpoints — query notifications and trigger retries.
///
/// Note: Notifications are primarily created by integration event consumers,
/// not by direct API calls. These endpoints are for querying and admin operations.
/// </summary>
public sealed class NotificationsController : ApiController
{
    // ── Queries ──────────────────────────────────────────────────────────

    /// <summary>
    /// Gets a notification by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotificationById(Guid id)
    {
        var query = new GetNotificationByIdQuery(id);
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Gets all notifications for the authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var query = new GetUserNotificationsQuery(userId);
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Gets all notifications for a specific user (admin only).
    /// </summary>
    [HttpGet("user/{userId}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserNotifications(string userId)
    {
        var query = new GetUserNotificationsQuery(userId);
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }

    // ── Admin Commands ───────────────────────────────────────────────────

    /// <summary>
    /// Manually triggers retry of failed notifications (admin only).
    /// Normally handled by the background retry service.
    /// </summary>
    [HttpPost("retry")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RetryFailedNotifications()
    {
        var command = new RetryFailedNotificationsCommand();
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping() => Ok(new { service = "StayHub.Notification", status = "healthy" });
}
