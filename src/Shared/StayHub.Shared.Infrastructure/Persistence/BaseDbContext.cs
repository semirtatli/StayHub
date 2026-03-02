using MediatR;
using Microsoft.EntityFrameworkCore;
using StayHub.Shared.Domain;
using StayHub.Shared.Interfaces;

namespace StayHub.Shared.Infrastructure.Persistence;

/// <summary>
/// Base DbContext for all StayHub microservices.
/// Each service creates its own DbContext inheriting from this.
///
/// Provides:
/// - IUnitOfWork implementation (SaveChangesAsync dispatches domain events)
/// - Automatic application of entity configurations from child assembly
/// - Domain event dispatching after save
///
/// Why not use interceptors for domain events? Because we need the dispatcher
/// to run AFTER SaveChanges succeeds (events should reflect persisted state),
/// and SaveChangesInterceptor ordering with multiple interceptors is fragile.
/// Overriding SaveChangesAsync is simpler and more explicit.
/// </summary>
public abstract class BaseDbContext : DbContext, IUnitOfWork
{
    private readonly IMediator _mediator;

    protected BaseDbContext(DbContextOptions options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Saves all changes and dispatches domain events raised by aggregate roots.
    /// Domain events are dispatched AFTER successful save to ensure consistency.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Save changes first (this runs the interceptors for audit/soft-delete)
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events after successful persistence
        await DispatchDomainEventsAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Collects and dispatches all domain events from tracked aggregate roots.
    /// Events are cleared after dispatch to prevent re-processing.
    ///
    /// Note: Events raised during event handling will be captured in the next
    /// SaveChangesAsync call — this prevents infinite recursion.
    /// </summary>
    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var aggregateRoots = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(ar => ar.DomainEvents)
            .ToList();

        // Clear events before dispatching to avoid re-entrance issues
        aggregateRoots.ForEach(ar => ar.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply global query filter for soft delete on all ISoftDeletable entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(
                        GenerateSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    /// <summary>
    /// Generates a lambda expression: entity => !((ISoftDeletable)entity).IsDeleted
    /// Applied as a global query filter so soft-deleted entities are excluded by default.
    /// Use .IgnoreQueryFilters() when you need to include deleted records.
    /// </summary>
    private static System.Linq.Expressions.LambdaExpression GenerateSoftDeleteFilter(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "entity");
        var isDeletedProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
        var notDeleted = System.Linq.Expressions.Expression.Not(isDeletedProperty);
        return System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);
    }
}
