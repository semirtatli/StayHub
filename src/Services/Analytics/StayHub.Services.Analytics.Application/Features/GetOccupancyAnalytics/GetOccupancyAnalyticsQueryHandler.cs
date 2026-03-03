using StayHub.Services.Analytics.Application.Abstractions;
using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Analytics.Application.Features.GetOccupancyAnalytics;

public sealed class GetOccupancyAnalyticsQueryHandler
    : IQueryHandler<GetOccupancyAnalyticsQuery, TimeSeriesResponseDto<OccupancyDataPointDto>>
{
    private readonly IAnalyticsQueryStore _queryStore;

    public GetOccupancyAnalyticsQueryHandler(IAnalyticsQueryStore queryStore)
    {
        _queryStore = queryStore;
    }

    public async Task<Result<TimeSeriesResponseDto<OccupancyDataPointDto>>> Handle(
        GetOccupancyAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.StartDate > request.EndDate)
        {
            return Result.Failure<TimeSeriesResponseDto<OccupancyDataPointDto>>(
                AnalyticsErrors.InvalidDateRange);
        }

        var dataPoints = await _queryStore.GetOccupancyTimeSeriesAsync(
            request.StartDate, request.EndDate, request.HotelId, cancellationToken);

        var response = new TimeSeriesResponseDto<OccupancyDataPointDto>(
            request.StartDate, request.EndDate, request.HotelId, dataPoints);

        return response;
    }
}
