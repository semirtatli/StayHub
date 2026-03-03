using Microsoft.EntityFrameworkCore;
using StayHub.Services.Analytics.Domain.Entities;
using StayHub.Services.Analytics.Domain.Repositories;

namespace StayHub.Services.Analytics.Infrastructure.Persistence.Repositories;

/// <summary>
/// Write-side repository for analytics projections.
/// Uses EF Core change tracking — modifications are committed by TransactionBehavior.
/// </summary>
public sealed class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AnalyticsDbContext _dbContext;

    public AnalyticsRepository(AnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ── Analytics Events ──
    public void AddEvent(AnalyticsEvent analyticsEvent)
    {
        _dbContext.AnalyticsEvents.Add(analyticsEvent);
    }

    // ── Revenue Snapshots ──
    public async Task<DailyRevenueSnapshot?> GetRevenueSnapshotAsync(
        Guid hotelId, DateOnly snapshotDate, CancellationToken cancellationToken)
    {
        return await _dbContext.DailyRevenueSnapshots
            .FirstOrDefaultAsync(
                s => s.HotelId == hotelId && s.Date == snapshotDate,
                cancellationToken);
    }

    public void AddRevenueSnapshot(DailyRevenueSnapshot snapshot)
    {
        _dbContext.DailyRevenueSnapshots.Add(snapshot);
    }

    // ── Occupancy Snapshots ──
    public async Task<OccupancySnapshot?> GetOccupancySnapshotAsync(
        Guid hotelId, DateOnly snapshotDate, CancellationToken cancellationToken)
    {
        return await _dbContext.OccupancySnapshots
            .FirstOrDefaultAsync(
                s => s.HotelId == hotelId && s.Date == snapshotDate,
                cancellationToken);
    }

    public void AddOccupancySnapshot(OccupancySnapshot snapshot)
    {
        _dbContext.OccupancySnapshots.Add(snapshot);
    }

    // ── Hotel Performance Summaries ──
    public async Task<HotelPerformanceSummary?> GetHotelPerformanceAsync(
        Guid hotelId, CancellationToken cancellationToken)
    {
        return await _dbContext.HotelPerformanceSummaries
            .FirstOrDefaultAsync(
                s => s.HotelId == hotelId,
                cancellationToken);
    }

    public void AddHotelPerformance(HotelPerformanceSummary summary)
    {
        _dbContext.HotelPerformanceSummaries.Add(summary);
    }
}
