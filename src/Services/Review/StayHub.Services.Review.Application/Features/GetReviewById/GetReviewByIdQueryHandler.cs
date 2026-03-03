using StayHub.Services.Review.Application.DTOs;
using StayHub.Services.Review.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Review.Application.Features.GetReviewById;

/// <summary>
/// Handles getting a review by ID.
/// </summary>
public sealed class GetReviewByIdQueryHandler : IQueryHandler<GetReviewByIdQuery, ReviewDto>
{
    private readonly IReviewRepository _reviewRepository;

    public GetReviewByIdQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<ReviewDto>> Handle(
        GetReviewByIdQuery request,
        CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(
            request.ReviewId, cancellationToken);

        if (review is null)
            return Result.Failure<ReviewDto>(ReviewErrors.Review.NotFound);

        return review.ToDto();
    }
}
