using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Review.Application.Features.DeleteReview;
using StayHub.Services.Review.Application.Features.GetHotelRatingSummary;
using StayHub.Services.Review.Application.Features.GetHotelReviews;
using StayHub.Services.Review.Application.Features.GetReviewById;
using StayHub.Services.Review.Application.Features.RespondToReview;
using StayHub.Services.Review.Application.Features.SubmitReview;
using StayHub.Services.Review.Application.Features.UpdateReview;

namespace StayHub.Services.Review.Api.Controllers;

/// <summary>
/// Review management endpoints — submit, update, delete reviews,
/// management responses, and rating aggregation.
/// </summary>
public sealed class ReviewsController : ApiController
{
    // ── Commands ──────────────────────────────────────────────────────────

    /// <summary>
    /// Submits a new review for a completed hotel stay.
    /// One review per booking per user.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitReview([FromBody] SubmitReviewRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new SubmitReviewCommand(
            request.HotelId,
            request.BookingId,
            request.GuestName,
            request.Title,
            request.Body,
            request.Cleanliness,
            request.Service,
            request.Location,
            request.Comfort,
            request.ValueForMoney,
            request.StayedFrom,
            request.StayedTo,
            userId);

        var result = await Mediator.Send(command);

        return HandleCreatedResult(result, nameof(GetReviewById), new { id = result.IsSuccess ? result.Value.Id : Guid.Empty });
    }

    /// <summary>
    /// Updates an existing review. Only the author can update.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReview(Guid id, [FromBody] UpdateReviewRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new UpdateReviewCommand(
            id,
            request.Title,
            request.Body,
            request.Cleanliness,
            request.Service,
            request.Location,
            request.Comfort,
            request.ValueForMoney,
            userId);

        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Soft-deletes a review. Only the author can delete.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new DeleteReviewCommand(id, userId);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Adds or updates a management response to a review.
    /// Only the hotel owner/admin can respond.
    /// </summary>
    [HttpPost("{id:guid}/respond")]
    [Authorize(Policy = "Authenticated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RespondToReview(Guid id, [FromBody] RespondToReviewRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new RespondToReviewCommand(id, request.Response, userId);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    // ── Queries ──────────────────────────────────────────────────────────

    /// <summary>
    /// Gets a review by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReviewById(Guid id)
    {
        var query = new GetReviewByIdQuery(id);
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Gets all reviews for a specific hotel.
    /// </summary>
    [HttpGet("hotel/{hotelId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHotelReviews(Guid hotelId)
    {
        var query = new GetHotelReviewsQuery(hotelId);
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Gets the aggregated rating summary for a hotel.
    /// </summary>
    [HttpGet("hotel/{hotelId:guid}/rating-summary")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHotelRatingSummary(Guid hotelId)
    {
        var query = new GetHotelRatingSummaryQuery(hotelId);
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping() => Ok(new { service = "StayHub.Review", status = "healthy" });
}

// ── Request DTOs (API-layer only — not shared) ──────────────────────────

/// <summary>
/// Request body for submitting a review.
/// </summary>
public sealed record SubmitReviewRequest(
    Guid HotelId,
    Guid BookingId,
    string GuestName,
    string Title,
    string Body,
    int Cleanliness,
    int Service,
    int Location,
    int Comfort,
    int ValueForMoney,
    DateOnly StayedFrom,
    DateOnly StayedTo);

/// <summary>
/// Request body for updating a review.
/// </summary>
public sealed record UpdateReviewRequest(
    string Title,
    string Body,
    int Cleanliness,
    int Service,
    int Location,
    int Comfort,
    int ValueForMoney);

/// <summary>
/// Request body for adding a management response.
/// </summary>
public sealed record RespondToReviewRequest(string Response);
