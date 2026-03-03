using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Analytics.Application.Features.GetOccupancyAnalytics;

/// <summary>
/// Retrieves daily occupancy time-series for the specified date range.
/// Optionally filtered by hotel.
/// </summary>
public sealed record GetOccupancyAnalyticsQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? HotelId = null) : IQuery<TimeSeriesResponseDto<OccupancyDataPointDto>>;
