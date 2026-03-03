using MediatR;
using Microsoft.EntityFrameworkCore;
using StayHub.Services.Analytics.Domain.Entities;
using StayHub.Shared.Infrastructure.Persistence;

namespace StayHub.Services.Analytics.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the Analytics Service.
/// Inherits BaseDbContext for outbox pattern + IUnitOfWork.
/// Projection entities (Entity-based, not AggregateRoot) are stored here.
/// </summary>
public sealed class AnalyticsDbContext : BaseDbContext
{
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();
    public DbSet<DailyRevenueSnapshot> DailyRevenueSnapshots => Set<DailyRevenueSnapshot>();
    public DbSet<OccupancySnapshot> OccupancySnapshots => Set<OccupancySnapshot>();
    public DbSet<HotelPerformanceSummary> HotelPerformanceSummaries => Set<HotelPerformanceSummary>();

    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options, IMediator mediator)
        : base(options, mediator)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);
    }
}
