using Microsoft.Extensions.Logging;
using StayHub.Services.Review.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Review.Application.Features.RespondToReview;

/// <summary>
/// Handles management response to a review.
/// For now, any authenticated user with HotelOwner role can respond (enforced at API layer).
/// In production, cross-service ownership check would verify via Hotel Service.
/// </summary>
public sealed class RespondToReviewCommandHandler : ICommandHandler<RespondToReviewCommand>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<RespondToReviewCommandHandler> _logger;

    public RespondToReviewCommandHandler(
        IReviewRepository reviewRepository,
        ILogger<RespondToReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        RespondToReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(
            request.ReviewId, cancellationToken);

        if (review is null)
            return Result.Failure(ReviewErrors.Review.NotFound);

        review.AddManagementResponse(request.Response);
        _reviewRepository.Update(review);

        _logger.LogInformation(
            "Management response added to review {ReviewId} by user {UserId}",
            review.Id, request.UserId);

        return Result.Success();
    }
}
