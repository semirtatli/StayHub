using StayHub.Services.Review.Application.DTOs;
using StayHub.Services.Review.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Review.Application.Features.GetHotelRatingSummary;

/// <summary>
/// Handles getting the cached rating summary for a hotel.
/// Returns zeros if no summary exists yet.
/// </summary>
public sealed class GetHotelRatingSummaryQueryHandler
    : IQueryHandler<GetHotelRatingSummaryQuery, HotelRatingSummaryDto>
{
    private readonly IReviewRepository _reviewRepository;

    public GetHotelRatingSummaryQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<HotelRatingSummaryDto>> Handle(
        GetHotelRatingSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await _reviewRepository.GetRatingSummaryByHotelIdAsync(
            request.HotelId, cancellationToken);

        if (summary is null)
        {
            // No reviews yet — return empty summary
            return new HotelRatingSummaryDto(
                request.HotelId, 0, 0, 0, 0, 0, 0, 0);
        }

        return summary.ToDto();
    }
}
