using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Application.Features.GetCancellationPolicy;
using StayHub.Services.Hotel.Application.Features.SetCancellationPolicy;

namespace StayHub.Services.Hotel.Api.Controllers;

/// <summary>
/// Cancellation policy management for hotels.
///
/// Authorization:
/// - GET /api/hotels/{hotelId}/cancellation-policy → AllowAnonymous (guests check before booking)
/// - PUT /api/hotels/{hotelId}/cancellation-policy → HotelOwnerOrAdmin (hotel owner sets policy)
/// </summary>
[Route("api/hotels/{hotelId:guid}/cancellation-policy")]
public sealed class CancellationPolicyController : ApiController
{
    /// <summary>
    /// Get the cancellation policy for a hotel.
    /// Public endpoint — guests can view the policy before booking.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CancellationPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        var query = new GetCancellationPolicyQuery(hotelId);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Set or update the cancellation policy for a hotel.
    /// Only the hotel owner can change the policy.
    ///
    /// Supports predefined types (Flexible/Moderate/Strict/NonRefundable)
    /// or custom configurations with explicit day/percentage values.
    /// </summary>
    [HttpPut]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(typeof(CancellationPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Set(
        Guid hotelId,
        [FromBody] SetCancellationPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new SetCancellationPolicyCommand(
            hotelId,
            request.PolicyType,
            request.UseCustom,
            request.FreeCancellationDays,
            request.PartialRefundPercentage,
            request.PartialRefundDays,
            ownerId);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────

/// <summary>
/// Request body for PUT /api/hotels/{hotelId}/cancellation-policy.
///
/// When UseCustom is false, the predefined defaults for PolicyType are used.
/// When UseCustom is true, all day/percentage fields are required.
/// </summary>
public sealed record SetCancellationPolicyRequest(
    string PolicyType,
    bool UseCustom = false,
    int? FreeCancellationDays = null,
    int? PartialRefundPercentage = null,
    int? PartialRefundDays = null);
