using MediatR;
using Microsoft.EntityFrameworkCore;
using StayHub.Services.Review.Domain.Entities;
using StayHub.Shared.Infrastructure.Persistence;

namespace StayHub.Services.Review.Infrastructure.Persistence;

/// <summary>
/// Review Service EF Core DbContext.
/// Inherits BaseDbContext which provides:
/// - IUnitOfWork implementation
/// - Domain event dispatching after SaveChanges
/// - Outbox message persistence
/// - Global soft-delete query filters
/// </summary>
public sealed class ReviewDbContext : BaseDbContext
{
    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();
    public DbSet<HotelRatingSummary> RatingSummaries => Set<HotelRatingSummary>();

    public ReviewDbContext(DbContextOptions<ReviewDbContext> options, IMediator mediator)
        : base(options, mediator)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReviewDbContext).Assembly);
    }
}
