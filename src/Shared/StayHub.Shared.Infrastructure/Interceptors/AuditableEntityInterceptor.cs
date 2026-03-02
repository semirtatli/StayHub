using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StayHub.Shared.Domain;
using StayHub.Shared.Interfaces;

namespace StayHub.Shared.Infrastructure.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets audit columns on save.
///
/// On Add: Sets CreatedAt and CreatedBy
/// On Modify: Sets LastModifiedAt and LastModifiedBy
///
/// Uses ICurrentUserService to get the authenticated user's ID.
/// Uses IDateTimeProvider for testable timestamps.
///
/// Registered in each service's DbContext configuration:
///   options.AddInterceptors(serviceProvider.GetRequiredService&lt;AuditableEntityInterceptor&gt;());
/// </summary>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditableEntityInterceptor(
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void UpdateAuditableEntities(DbContext context)
    {
        var utcNow = _dateTimeProvider.UtcNow;
        var userId = _currentUserService.UserId;

        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = utcNow;
                    entry.Entity.LastModifiedBy = userId;
                    break;
            }
        }
    }
}
