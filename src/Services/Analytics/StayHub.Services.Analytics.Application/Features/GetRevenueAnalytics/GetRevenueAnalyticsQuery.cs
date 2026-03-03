using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Analytics.Application.Features.GetRevenueAnalytics;

/// <summary>
/// Retrieves daily revenue time-series for the specified date range.
/// Optionally filtered by hotel.
/// </summary>
public sealed record GetRevenueAnalyticsQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? HotelId = null) : IQuery<TimeSeriesResponseDto<RevenueDataPointDto>>;
