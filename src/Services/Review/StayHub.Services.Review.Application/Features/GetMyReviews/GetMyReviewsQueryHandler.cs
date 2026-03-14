using StayHub.Services.Review.Application.DTOs;
using StayHub.Services.Review.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Review.Application.Features.GetMyReviews;

public sealed class GetMyReviewsQueryHandler
    : IQueryHandler<GetMyReviewsQuery, IReadOnlyList<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetMyReviewsQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> Handle(
        GetMyReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByUserIdAsync(
            request.UserId, cancellationToken);

        var dtos = reviews.Select(r => r.ToDto()).ToList();

        return Result.Success<IReadOnlyList<ReviewDto>>(dtos);
    }
}
