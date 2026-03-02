using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StayHub.Shared.Domain;
using StayHub.Shared.Interfaces;

namespace StayHub.Shared.Infrastructure.Interceptors;

/// <summary>
/// EF Core interceptor that converts hard deletes to soft deletes.
///
/// When EF Core detects an entity marked as Deleted:
/// - If it implements ISoftDeletable, changes state to Modified
/// - Sets IsDeleted = true, DeletedAt, DeletedBy
/// - The entity stays in the database but is filtered out by global query filters
///
/// Combined with BaseDbContext's global query filter (WHERE IsDeleted = 0),
/// this provides transparent soft-delete behavior:
/// - repository.Remove(entity) marks it as deleted, doesn't physically remove it
/// - All queries automatically exclude deleted entities
/// - Use .IgnoreQueryFilters() for admin/audit views that need deleted records
/// </summary>
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SoftDeleteInterceptor(
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
            ConvertDeleteToSoftDelete(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            ConvertDeleteToSoftDelete(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void ConvertDeleteToSoftDelete(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            // Convert physical delete to soft delete
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = _dateTimeProvider.UtcNow;
            entry.Entity.DeletedBy = _currentUserService.UserId;
        }
    }
}
