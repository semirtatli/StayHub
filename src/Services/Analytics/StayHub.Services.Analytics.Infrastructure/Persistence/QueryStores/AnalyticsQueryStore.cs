using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StayHub.Services.Analytics.Application.Abstractions;
using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Services.Analytics.Domain.Enums;

namespace StayHub.Services.Analytics.Infrastructure.Persistence.QueryStores;

/// <summary>
/// Read-side query store implementation using EF Core LINQ-to-SQL.
/// Translates DTO-oriented queries directly against the database for performance.
/// This is the CQRS read model — bypasses the domain layer.
/// </summary>
public sealed class AnalyticsQueryStore : IAnalyticsQueryStore
{
    private readonly AnalyticsDbContext _dbContext;

    public AnalyticsQueryStore(AnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RevenueDataPointDto>> GetRevenueTimeSeriesAsync(
        DateOnly startDate,
        DateOnly endDate,
        Guid? hotelId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.DailyRevenueSnapshots
            .Where(s => s.Date >= startDate && s.Date <= endDate);

        if (hotelId.HasValue)
        {
            query = query.Where(s => s.HotelId == hotelId.Value);
        }

        // Group by date to aggregate across hotels when no filter
        var result = await query
            .GroupBy(s => s.Date)
            .Select(g => new RevenueDataPointDto(
                g.Key,
                g.Sum(s => s.TotalRevenue),
                g.Sum(s => s.BookingCount),
                g.Sum(s => s.BookingCount) > 0
                    ? g.Sum(s => s.TotalRevenue) / g.Sum(s => s.BookingCount)
                    : 0,
                g.Sum(s => s.CancellationCount),
                g.Sum(s => s.RefundAmount)))
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<OccupancyDataPointDto>> GetOccupancyTimeSeriesAsync(
        DateOnly startDate,
        DateOnly endDate,
        Guid? hotelId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.OccupancySnapshots
            .Where(s => s.Date >= startDate && s.Date <= endDate);

        if (hotelId.HasValue)
        {
            query = query.Where(s => s.HotelId == hotelId.Value);
        }

        var result = await query
            .GroupBy(s => s.Date)
            .Select(g => new OccupancyDataPointDto(
                g.Key,
                g.Sum(s => s.TotalRooms) > 0
                    ? (decimal)g.Sum(s => s.BookedRooms) / g.Sum(s => s.TotalRooms) * 100m
                    : 0,
                g.Sum(s => s.BookedRooms),
                g.Sum(s => s.TotalRooms)))
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<BookingTrendDto>> GetBookingTrendsAsync(
        DateOnly startDate,
        DateOnly endDate,
        TimePeriod period,
        Guid? hotelId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.DailyRevenueSnapshots
            .Where(s => s.Date >= startDate && s.Date <= endDate);

        if (hotelId.HasValue)
        {
            query = query.Where(s => s.HotelId == hotelId.Value);
        }

        // Fetch data and group in memory for flexible period grouping
        var snapshots = await query.ToListAsync(cancellationToken);

        var grouped = snapshots
            .GroupBy(s => GetPeriodKey(s.Date, period))
            .Select(g => new BookingTrendDto(
                g.Key,
                g.Sum(s => s.BookingCount),
                g.Sum(s => s.CancellationCount),
                g.Sum(s => s.TotalRevenue)))
            .OrderBy(t => t.Period)
            .ToList();

        return grouped;
    }

    public async Task<IReadOnlyList<HotelPerformanceDto>> GetTopHotelsAsync(
        string metricType,
        int count,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.HotelPerformanceSummaries.AsQueryable();

        var ordered = metricType.ToUpperInvariant() switch
        {
            "REVENUE" => query.OrderByDescending(h => h.TotalRevenue),
            "BOOKINGS" => query.OrderByDescending(h => h.TotalBookings),
            "RATING" => query.OrderByDescending(h => h.AverageRating),
            "REVIEWS" => query.OrderByDescending(h => h.TotalReviews),
            _ => query.OrderByDescending(h => h.TotalRevenue)
        };

        var result = await ordered
            .Take(count)
            .Select(h => new HotelPerformanceDto(
                h.HotelId,
                h.HotelName,
                h.TotalRevenue,
                h.TotalBookings,
                h.AverageRating,
                h.TotalReviews,
                h.CancellationRate,
                h.AverageOccupancyRate))
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<DashboardKpiDto> GetDashboardKpisAsync(
        CancellationToken cancellationToken)
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDaysAgo = now.AddDays(-30);
        var sixtyDaysAgo = now.AddDays(-60);

        // Current period (last 30 days)
        var currentSnapshots = await _dbContext.DailyRevenueSnapshots
            .Where(s => s.Date >= thirtyDaysAgo && s.Date <= now)
            .ToListAsync(cancellationToken);

        // Previous period (30-60 days ago)
        var previousSnapshots = await _dbContext.DailyRevenueSnapshots
            .Where(s => s.Date >= sixtyDaysAgo && s.Date < thirtyDaysAgo)
            .ToListAsync(cancellationToken);

        var currentRevenue = currentSnapshots.Sum(s => s.TotalRevenue);
        var previousRevenue = previousSnapshots.Sum(s => s.TotalRevenue);
        var currentBookings = currentSnapshots.Sum(s => s.BookingCount);
        var previousBookings = previousSnapshots.Sum(s => s.BookingCount);
        var currentCancellations = currentSnapshots.Sum(s => s.CancellationCount);

        var revenueChangePercent = previousRevenue > 0
            ? (currentRevenue - previousRevenue) / previousRevenue * 100m
            : 0;

        var bookingChangePercent = previousBookings > 0
            ? (decimal)(currentBookings - previousBookings) / previousBookings * 100m
            : 0;

        // Aggregate from hotel performance summaries
        var summaries = await _dbContext.HotelPerformanceSummaries
            .ToListAsync(cancellationToken);

        var totalRevenue = summaries.Sum(s => s.TotalRevenue);
        var totalBookings = summaries.Sum(s => s.TotalBookings);
        var totalCancellations = summaries.Sum(s => s.TotalCancellations);
        var totalReviews = summaries.Sum(s => s.TotalReviews);
        var avgRating = summaries.Count > 0
            ? summaries.Where(s => s.TotalReviews > 0).DefaultIfEmpty().Average(s => s?.AverageRating ?? 0)
            : 0;
        var avgOccupancy = summaries.Count > 0
            ? summaries.Average(s => s.AverageOccupancyRate)
            : 0;
        var cancellationRate = totalBookings > 0
            ? (decimal)totalCancellations / totalBookings * 100m
            : 0;
        var avgBookingValue = totalBookings > 0
            ? totalRevenue / totalBookings
            : 0;

        return new DashboardKpiDto(
            TotalRevenue: totalRevenue,
            TotalBookings: totalBookings,
            TotalCancellations: totalCancellations,
            CancellationRate: cancellationRate,
            AverageBookingValue: avgBookingValue,
            AverageRating: (decimal)avgRating,
            TotalReviews: totalReviews,
            AverageOccupancyRate: avgOccupancy,
            ActiveHotels: summaries.Count,
            RevenueChangePercent: revenueChangePercent,
            BookingChangePercent: bookingChangePercent);
    }

    public async Task<byte[]> ExportRevenueDataAsync(
        DateOnly startDate,
        DateOnly endDate,
        Guid? hotelId,
        CancellationToken cancellationToken)
    {
        var dataPoints = await GetRevenueTimeSeriesAsync(
            startDate, endDate, hotelId, cancellationToken);

        if (dataPoints.Count == 0)
        {
            return [];
        }

        var sb = new StringBuilder();
        sb.AppendLine("Date,Revenue,BookingCount,AverageBookingValue,CancellationCount,RefundAmount");

        foreach (var dp in dataPoints)
        {
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"{dp.Date:yyyy-MM-dd},{dp.Revenue:F2},{dp.BookingCount},{dp.AverageBookingValue:F2},{dp.CancellationCount},{dp.RefundAmount:F2}"));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    /// <summary>
    /// Groups a date into a period key string for trend aggregation.
    /// </summary>
    private static string GetPeriodKey(DateOnly date, TimePeriod period)
    {
        return period switch
        {
            TimePeriod.Daily => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TimePeriod.Weekly => $"{date.Year}-W{CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                date.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday):D2}",
            TimePeriod.Monthly => date.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            TimePeriod.Yearly => date.Year.ToString(CultureInfo.InvariantCulture),
            _ => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };
    }
}
