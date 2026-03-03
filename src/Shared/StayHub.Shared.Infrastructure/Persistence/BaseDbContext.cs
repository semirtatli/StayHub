using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayHub.Shared.Domain;
using StayHub.Shared.Infrastructure.Outbox;
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
/// - Outbox pattern: domain events are serialized and stored in the OutboxMessages
///   table within the SAME database transaction as entity changes, solving the
///   dual-write problem (see ADR-006)
///
/// Why not use interceptors for domain events? Because we need the dispatcher
/// to run AFTER SaveChanges succeeds (events should reflect persisted state),
/// and SaveChangesInterceptor ordering with multiple interceptors is fragile.
/// Overriding SaveChangesAsync is simpler and more explicit.
/// </summary>
public abstract class BaseDbContext : DbContext, IUnitOfWork
{
    private readonly IMediator _mediator;

    private static readonly JsonSerializerOptions OutboxJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    protected BaseDbContext(DbContextOptions options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Outbox table for reliable messaging. Each service's DB has its own outbox.
    /// Background processor polls this table and publishes to the message broker.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Saves all changes, stores domain events as outbox messages (atomically),
    /// and dispatches domain events in-process via MediatR for local handlers.
    ///
    /// Order matters:
    /// 1. Collect domain events from tracked aggregates (and clear them)
    /// 2. Serialize events as OutboxMessage rows (added to change tracker)
    /// 3. base.SaveChangesAsync — persists BOTH entity changes AND outbox messages
    ///    in a single transaction (solves dual-write problem)
    /// 4. Dispatch domain events in-process for same-service handlers
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Collect domain events from tracked aggregate roots
        var domainEvents = CollectDomainEvents();

        // 2. Serialize as outbox messages (added to change tracker, persisted atomically)
        AddOutboxMessages(domainEvents);

        // 3. Persist everything in one transaction (entities + outbox messages)
        var result = await base.SaveChangesAsync(cancellationToken);

        // 4. Dispatch domain events in-process via MediatR for local handlers
        await DispatchDomainEventsAsync(domainEvents, cancellationToken);

        return result;
    }

    /// <summary>
    /// Collects and removes all domain events from tracked aggregate roots.
    /// Events are cleared before save to prevent re-processing on subsequent saves.
    /// </summary>
    private List<IDomainEvent> CollectDomainEvents()
    {
        var aggregateRoots = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(ar => ar.DomainEvents)
            .ToList();

        // Clear events before persisting to avoid re-entrance issues
        aggregateRoots.ForEach(ar => ar.ClearDomainEvents());

        return domainEvents;
    }

    /// <summary>
    /// Converts domain events to OutboxMessage entities and adds them to the change tracker.
    /// They will be saved in the same transaction as the entity changes.
    /// </summary>
    private void AddOutboxMessages(List<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            var type = domainEvent.GetType();
            var eventType = $"{type.FullName}, {type.Assembly.GetName().Name}";
            var payload = JsonSerializer.Serialize(domainEvent, type, OutboxJsonOptions);

            var outboxMessage = OutboxMessage.Create(eventType, payload);
            OutboxMessages.Add(outboxMessage);
        }
    }

    /// <summary>
    /// Dispatches domain events in-process via MediatR.
    /// These are for local handlers within the same service (e.g., sending notifications,
    /// updating read models). Cross-service communication goes through the outbox → broker.
    /// </summary>
    private async Task DispatchDomainEventsAsync(
        List<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply outbox message configuration (shared across all service DbContexts)
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

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
