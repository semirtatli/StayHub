using Microsoft.Extensions.Logging;
using StayHub.Services.Review.Domain.Events;
using StayHub.Services.Review.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Review.Application.Features.DeleteReview;

/// <summary>
/// Handles review soft-deletion — sets IsDeleted flag via aggregate root.
/// Raises ReviewDeletedEvent for rating recalculation.
/// TransactionBehavior commits the unit of work after a successful result.
/// </summary>
public sealed class DeleteReviewCommandHandler : ICommandHandler<DeleteReviewCommand>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<DeleteReviewCommandHandler> _logger;

    public DeleteReviewCommandHandler(
        IReviewRepository reviewRepository,
        ILogger<DeleteReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(
            request.ReviewId, cancellationToken);

        if (review is null)
            return Result.Failure(ReviewErrors.Review.NotFound);

        if (review.UserId != request.UserId)
            return Result.Failure(ReviewErrors.Review.NotAuthor);

        // Soft-delete through the aggregate root
        review.IsDeleted = true;
        review.DeletedAt = DateTime.UtcNow;
        review.DeletedBy = request.UserId;

        _reviewRepository.Update(review);

        _logger.LogInformation(
            "Review {ReviewId} soft-deleted by user {UserId}",
            review.Id, request.UserId);

        return Result.Success();
    }
}
