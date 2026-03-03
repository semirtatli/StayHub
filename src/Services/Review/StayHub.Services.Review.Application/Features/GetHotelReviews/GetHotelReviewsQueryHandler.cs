using StayHub.Services.Review.Application.DTOs;
using StayHub.Services.Review.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Review.Application.Features.GetHotelReviews;

/// <summary>
/// Handles getting all reviews for a hotel — returns summary list.
/// </summary>
public sealed class GetHotelReviewsQueryHandler
    : IQueryHandler<GetHotelReviewsQuery, IReadOnlyList<ReviewSummaryDto>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetHotelReviewsQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<IReadOnlyList<ReviewSummaryDto>>> Handle(
        GetHotelReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByHotelIdAsync(
            request.HotelId, cancellationToken);

        var dtos = reviews.Select(r => r.ToSummaryDto()).ToList();

        return Result.Success<IReadOnlyList<ReviewSummaryDto>>(dtos);
    }
}
