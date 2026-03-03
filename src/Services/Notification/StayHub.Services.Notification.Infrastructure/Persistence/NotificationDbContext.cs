using MediatR;
using Microsoft.EntityFrameworkCore;
using StayHub.Services.Notification.Domain.Entities;
using StayHub.Shared.Infrastructure.Persistence;

namespace StayHub.Services.Notification.Infrastructure.Persistence;

/// <summary>
/// Notification Service EF Core DbContext.
/// Inherits BaseDbContext which provides:
/// - IUnitOfWork implementation
/// - Domain event dispatching after SaveChanges
/// - Outbox message persistence
/// - Global soft-delete query filters
/// </summary>
public sealed class NotificationDbContext : BaseDbContext
{
    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options, IMediator mediator)
        : base(options, mediator)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
