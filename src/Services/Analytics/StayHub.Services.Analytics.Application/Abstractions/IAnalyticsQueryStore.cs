using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Services.Analytics.Domain.Enums;

namespace StayHub.Services.Analytics.Application.Abstractions;

/// <summary>
/// Read-side query store for analytics data (CQRS read model).
/// Implemented by the Infrastructure layer using EF Core LINQ-to-SQL.
/// Query handlers depend on this instead of DbContext directly.
/// </summary>
public interface IAnalyticsQueryStore
{
    /// <summary>
    /// Revenue time-series for the given date range, optionally filtered by hotel.
    /// </summary>
    Task<IReadOnlyList<RevenueDataPointDto>> GetRevenueTimeSeriesAsync(
        DateOnly startDate,
        DateOnly endDate,
        Guid? hotelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Occupancy time-series for the given date range, optionally filtered by hotel.
    /// </summary>
    Task<IReadOnlyList<OccupancyDataPointDto>> GetOccupancyTimeSeriesAsync(
        DateOnly startDate,
        DateOnly endDate,
        Guid? hotelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Booking trends aggregated by the specified time period.
    /// </summary>
    Task<IReadOnlyList<BookingTrendDto>> GetBookingTrendsAsync(
        DateOnly startDate,
        DateOnly endDate,
        TimePeriod period,
        Guid? hotelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Top performing hotels ranked by the specified metric.
    /// </summary>
    Task<IReadOnlyList<HotelPerformanceDto>> GetTopHotelsAsync(
        string metricType,
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggregated KPI summary for the admin dashboard, including
    /// period-over-period comparison (last 30 days vs previous 30 days).
    /// </summary>
    Task<DashboardKpiDto> GetDashboardKpisAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a CSV export of revenue analytics for the given date range.
    /// </summary>
    Task<byte[]> ExportRevenueDataAsync(
        DateOnly startDate,
        DateOnly endDate,
        Guid? hotelId,
        CancellationToken cancellationToken = default);
}
