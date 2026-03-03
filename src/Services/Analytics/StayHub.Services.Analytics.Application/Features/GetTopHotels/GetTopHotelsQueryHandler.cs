using StayHub.Services.Analytics.Application.Abstractions;
using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Analytics.Application.Features.GetTopHotels;

public sealed class GetTopHotelsQueryHandler
    : IQueryHandler<GetTopHotelsQuery, IReadOnlyList<HotelPerformanceDto>>
{
    private readonly IAnalyticsQueryStore _queryStore;

    public GetTopHotelsQueryHandler(IAnalyticsQueryStore queryStore)
    {
        _queryStore = queryStore;
    }

    public async Task<Result<IReadOnlyList<HotelPerformanceDto>>> Handle(
        GetTopHotelsQuery request,
        CancellationToken cancellationToken)
    {
        var validMetrics = new[] { "Revenue", "Bookings", "Rating", "Reviews" };
        if (!validMetrics.Contains(request.MetricType, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure<IReadOnlyList<HotelPerformanceDto>>(
                AnalyticsErrors.InvalidMetricType);
        }

        var hotels = await _queryStore.GetTopHotelsAsync(
            request.MetricType, request.Count, cancellationToken);

        return Result.Success<IReadOnlyList<HotelPerformanceDto>>(hotels);
    }
}
