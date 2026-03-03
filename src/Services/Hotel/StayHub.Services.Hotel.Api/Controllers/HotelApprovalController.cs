using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Application.Features.ApproveHotel;
using StayHub.Services.Hotel.Application.Features.CloseHotel;
using StayHub.Services.Hotel.Application.Features.GetPendingApprovals;
using StayHub.Services.Hotel.Application.Features.ReactivateHotel;
using StayHub.Services.Hotel.Application.Features.RejectHotel;
using StayHub.Services.Hotel.Application.Features.SubmitForApproval;
using StayHub.Services.Hotel.Application.Features.SuspendHotel;

namespace StayHub.Services.Hotel.Api.Controllers;

/// <summary>
/// Hotel approval workflow controller — manages the hotel lifecycle state machine.
///
/// State machine:
///   Draft → PendingApproval → Active / Rejected
///   Active ↔ Suspended
///   Any (except Closed) → Closed (terminal)
///
/// Authorization:
/// - POST submit → HotelOwnerOrAdmin (owner submits own hotel)
/// - POST approve/reject/reactivate → AdminOnly
/// - POST suspend/close → HotelOwnerOrAdmin (owner or admin)
/// - GET pending → AdminOnly (admin review queue)
/// </summary>
[Route("api/hotels")]
public sealed class HotelApprovalController : ApiController
{
    /// <summary>
    /// Submit a hotel for admin approval. Owner only.
    /// Transitions: Draft → PendingApproval, Rejected → PendingApproval.
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitForApproval(
        Guid id,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new SubmitForApprovalCommand(id, ownerId);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Approve a hotel listing. Admin only.
    /// Transition: PendingApproval → Active.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        Guid id,
        CancellationToken cancellationToken)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new ApproveHotelCommand(id, adminUserId);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Reject a hotel listing. Admin only. Requires a reason.
    /// Transition: PendingApproval → Rejected.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectHotelRequest request,
        CancellationToken cancellationToken)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new RejectHotelCommand(id, request.Reason, adminUserId);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Suspend an active hotel. Owner or admin.
    /// Transition: Active → Suspended.
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspend(
        Guid id,
        [FromBody] SuspendHotelRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new SuspendHotelCommand(id, request.Reason, userId);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Reactivate a suspended hotel. Admin only.
    /// Transition: Suspended → Active.
    /// </summary>
    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new ReactivateHotelCommand(id, adminUserId);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Permanently close a hotel. Owner or admin. This is a terminal state.
    /// Transition: Any (except Closed) → Closed.
    /// </summary>
    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(
        Guid id,
        [FromBody] CloseHotelRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new CloseHotelCommand(id, request.Reason, userId);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Get all hotels pending admin approval. Admin only.
    /// Returns summary DTOs for the admin review queue.
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(IReadOnlyList<HotelSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingApprovals(
        CancellationToken cancellationToken)
    {
        var query = new GetPendingApprovalsQuery();
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────

/// <summary>Request body for rejecting a hotel.</summary>
public sealed record RejectHotelRequest(string Reason);

/// <summary>Request body for suspending a hotel.</summary>
public sealed record SuspendHotelRequest(string? Reason);

/// <summary>Request body for closing a hotel.</summary>
public sealed record CloseHotelRequest(string? Reason);
