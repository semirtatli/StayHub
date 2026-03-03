using StayHub.Services.Analytics.Application.Abstractions;
using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Analytics.Application.Features.GetBookingTrends;

public sealed class GetBookingTrendsQueryHandler
    : IQueryHandler<GetBookingTrendsQuery, IReadOnlyList<BookingTrendDto>>
{
    private readonly IAnalyticsQueryStore _queryStore;

    public GetBookingTrendsQueryHandler(IAnalyticsQueryStore queryStore)
    {
        _queryStore = queryStore;
    }

    public async Task<Result<IReadOnlyList<BookingTrendDto>>> Handle(
        GetBookingTrendsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.StartDate > request.EndDate)
        {
            return Result.Failure<IReadOnlyList<BookingTrendDto>>(
                AnalyticsErrors.InvalidDateRange);
        }

        var trends = await _queryStore.GetBookingTrendsAsync(
            request.StartDate, request.EndDate, request.Period, request.HotelId, cancellationToken);

        return Result.Success<IReadOnlyList<BookingTrendDto>>(trends);
    }
}
