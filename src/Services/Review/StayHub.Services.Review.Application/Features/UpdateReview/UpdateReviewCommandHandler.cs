using Microsoft.Extensions.Logging;
using StayHub.Services.Review.Application.DTOs;
using StayHub.Services.Review.Domain.Repositories;
using StayHub.Services.Review.Domain.ValueObjects;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Review.Application.Features.UpdateReview;

/// <summary>
/// Handles review update — verifies author ownership, updates title/body/rating.
/// TransactionBehavior commits the unit of work after a successful result.
/// </summary>
public sealed class UpdateReviewCommandHandler
    : ICommandHandler<UpdateReviewCommand, ReviewDto>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<UpdateReviewCommandHandler> _logger;

    public UpdateReviewCommandHandler(
        IReviewRepository reviewRepository,
        ILogger<UpdateReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<Result<ReviewDto>> Handle(
        UpdateReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(
            request.ReviewId, cancellationToken);

        if (review is null)
            return Result.Failure<ReviewDto>(ReviewErrors.Review.NotFound);

        if (review.UserId != request.UserId)
            return Result.Failure<ReviewDto>(ReviewErrors.Review.NotAuthor);

        var rating = Rating.Create(
            request.Cleanliness,
            request.Service,
            request.Location,
            request.Comfort,
            request.ValueForMoney);

        review.Update(request.Title, request.Body, rating);
        _reviewRepository.Update(review);

        _logger.LogInformation(
            "Review {ReviewId} updated — New overall: {Overall}",
            review.Id, rating.Overall);

        return review.ToDto();
    }
}
