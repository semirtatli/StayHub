using Microsoft.Extensions.Logging;
using StayHub.Services.Review.Application.DTOs;
using StayHub.Services.Review.Domain.Entities;
using StayHub.Services.Review.Domain.Repositories;
using StayHub.Services.Review.Domain.ValueObjects;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Review.Application.Features.SubmitReview;

/// <summary>
/// Handles review submission — creates a ReviewEntity aggregate.
///
/// Validates that the user hasn't already reviewed this booking.
/// Booking completion check is enforced at this layer (no cross-service call for now —
/// the booking reference is trusted from the frontend/BFF).
///
/// TransactionBehavior commits the unit of work after a successful result.
/// </summary>
public sealed class SubmitReviewCommandHandler
    : ICommandHandler<SubmitReviewCommand, ReviewDto>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<SubmitReviewCommandHandler> _logger;

    public SubmitReviewCommandHandler(
        IReviewRepository reviewRepository,
        ILogger<SubmitReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<Result<ReviewDto>> Handle(
        SubmitReviewCommand request,
        CancellationToken cancellationToken)
    {
        // Check if user already reviewed this booking
        var alreadyReviewed = await _reviewRepository.HasUserReviewedBookingAsync(
            request.UserId, request.BookingId, cancellationToken);

        if (alreadyReviewed)
            return Result.Failure<ReviewDto>(ReviewErrors.Review.AlreadyReviewed);

        // Create the rating value object
        var rating = Rating.Create(
            request.Cleanliness,
            request.Service,
            request.Location,
            request.Comfort,
            request.ValueForMoney);

        // Create the review aggregate
        var review = ReviewEntity.Create(
            request.HotelId,
            request.BookingId,
            request.UserId,
            request.GuestName,
            request.Title,
            request.Body,
            rating,
            request.StayedFrom,
            request.StayedTo);

        _reviewRepository.Add(review);

        _logger.LogInformation(
            "Review {ReviewId} submitted for hotel {HotelId} by user {UserId} — Overall: {Overall}",
            review.Id, request.HotelId, request.UserId, rating.Overall);

        return review.ToDto();
    }
}
