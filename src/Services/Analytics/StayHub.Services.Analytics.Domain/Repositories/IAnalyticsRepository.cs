using StayHub.Services.Analytics.Domain.Entities;

namespace StayHub.Services.Analytics.Domain.Repositories;

/// <summary>
/// Write-side repository for analytics projections.
/// Used by event-projection command handlers to create/update read models.
/// Query handlers use IAnalyticsQueryStore instead (CQRS read-side).
/// </summary>
public interface IAnalyticsRepository
{
    // ── Analytics Events (append-only) ──
    void AddEvent(AnalyticsEvent analyticsEvent);

    // ── Daily Revenue Snapshots ──
    Task<DailyRevenueSnapshot?> GetRevenueSnapshotAsync(
        Guid hotelId, DateOnly snapshotDate, CancellationToken cancellationToken = default);

    void AddRevenueSnapshot(DailyRevenueSnapshot snapshot);

    // ── Occupancy Snapshots ──
    Task<OccupancySnapshot?> GetOccupancySnapshotAsync(
        Guid hotelId, DateOnly snapshotDate, CancellationToken cancellationToken = default);

    void AddOccupancySnapshot(OccupancySnapshot snapshot);

    // ── Hotel Performance Summaries ──
    Task<HotelPerformanceSummary?> GetHotelPerformanceAsync(
        Guid hotelId, CancellationToken cancellationToken = default);

    void AddHotelPerformance(HotelPerformanceSummary summary);
}
