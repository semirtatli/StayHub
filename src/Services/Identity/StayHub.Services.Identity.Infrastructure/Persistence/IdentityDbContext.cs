using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StayHub.Services.Identity.Domain.Entities;
using StayHub.Services.Identity.Infrastructure.Identity;
using StayHub.Shared.Domain;
using StayHub.Shared.Interfaces;

namespace StayHub.Services.Identity.Infrastructure.Persistence;

/// <summary>
/// Identity Service DbContext — implements IUnitOfWork for transactional consistency.
///
/// Inherits from IdentityDbContext to get ASP.NET Core Identity tables:
/// - AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, etc.
///
/// Adds StayHub-specific tables:
/// - RefreshTokens (for JWT rotation)
///
/// Domain event dispatching happens in SaveChangesAsync (inherited pattern from BaseDbContext,
/// but here we override directly since we inherit from IdentityDbContext, not BaseDbContext).
/// </summary>
public class IdentityDbContext : IdentityDbContext<ApplicationUser>, IUnitOfWork
{
    private readonly IMediator _mediator;

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events after successful save
        await DispatchDomainEventsAsync(cancellationToken);

        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entitiesWithEvents = ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity is AggregateRoot)
            .Select(e => (AggregateRoot)e.Entity)
            .Where(ar => ar.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(ar => ar.DomainEvents)
            .ToList();

        entitiesWithEvents.ForEach(ar => ar.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all entity configurations from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        // Global soft-delete query filter for ISoftDeletable entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "entity");
                var isDeletedProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var notDeleted = System.Linq.Expressions.Expression.Not(isDeletedProperty);
                var lambda = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);

                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
