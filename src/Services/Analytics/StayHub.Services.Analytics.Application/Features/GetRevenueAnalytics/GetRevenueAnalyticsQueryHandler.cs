using StayHub.Services.Analytics.Application.Abstractions;
using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Analytics.Application.Features.GetRevenueAnalytics;

public sealed class GetRevenueAnalyticsQueryHandler
    : IQueryHandler<GetRevenueAnalyticsQuery, TimeSeriesResponseDto<RevenueDataPointDto>>
{
    private readonly IAnalyticsQueryStore _queryStore;

    public GetRevenueAnalyticsQueryHandler(IAnalyticsQueryStore queryStore)
    {
        _queryStore = queryStore;
    }

    public async Task<Result<TimeSeriesResponseDto<RevenueDataPointDto>>> Handle(
        GetRevenueAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.StartDate > request.EndDate)
        {
            return Result.Failure<TimeSeriesResponseDto<RevenueDataPointDto>>(
                AnalyticsErrors.InvalidDateRange);
        }

        var dataPoints = await _queryStore.GetRevenueTimeSeriesAsync(
            request.StartDate, request.EndDate, request.HotelId, cancellationToken);

        var response = new TimeSeriesResponseDto<RevenueDataPointDto>(
            request.StartDate, request.EndDate, request.HotelId, dataPoints);

        return response;
    }
}
