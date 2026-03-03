using StayHub.Services.Analytics.Domain.Enums;

namespace StayHub.Services.Analytics.Application.DTOs;

/// <summary>
/// Revenue data for a single day — used in time-series charts.
/// </summary>
public sealed record RevenueDataPointDto(
    DateOnly Date,
    decimal Revenue,
    int BookingCount,
    decimal AverageBookingValue,
    int CancellationCount,
    decimal RefundAmount);

/// <summary>
/// Occupancy data for a single day — used in occupancy charts.
/// </summary>
public sealed record OccupancyDataPointDto(
    DateOnly Date,
    decimal OccupancyRate,
    int BookedRooms,
    int TotalRooms);

/// <summary>
/// Booking trend aggregated by period (daily/weekly/monthly/yearly).
/// </summary>
public sealed record BookingTrendDto(
    string Period,
    int BookingCount,
    int CancellationCount,
    decimal Revenue);

/// <summary>
/// Hotel ranking / performance card DTO for "Top Hotels" views.
/// </summary>
public sealed record HotelPerformanceDto(
    Guid HotelId,
    string HotelName,
    decimal TotalRevenue,
    int TotalBookings,
    decimal AverageRating,
    int TotalReviews,
    decimal CancellationRate,
    decimal OccupancyRate);

/// <summary>
/// Aggregated KPI summary for the admin dashboard.
/// </summary>
public sealed record DashboardKpiDto(
    decimal TotalRevenue,
    int TotalBookings,
    int TotalCancellations,
    decimal CancellationRate,
    decimal AverageBookingValue,
    decimal AverageRating,
    int TotalReviews,
    decimal AverageOccupancyRate,
    int ActiveHotels,
    decimal RevenueChangePercent,
    decimal BookingChangePercent);

/// <summary>
/// Wrapper for time-series query responses with metadata.
/// </summary>
public sealed record TimeSeriesResponseDto<T>(
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? HotelId,
    IReadOnlyList<T> DataPoints);

/// <summary>
/// Analytics export result containing the file bytes and metadata.
/// </summary>
public sealed record AnalyticsExportDto(
    byte[] FileContent,
    string FileName,
    string ContentType);
