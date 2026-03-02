using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StayHub.Shared.Domain;
using StayHub.Shared.Interfaces;

namespace StayHub.Shared.Infrastructure.Persistence;

/// <summary>
/// Generic EF Core repository implementation for aggregate roots.
/// Each microservice creates typed repositories that inherit from this:
///
///   public class HotelRepository : Repository&lt;Hotel&gt;, IHotelRepository
///
/// This base class provides standard CRUD operations.
/// Service-specific queries go in the derived repository.
/// </summary>
public class Repository<T> : IRepository<T> where T : AggregateRoot
{
    protected DbContext DbContext { get; }
    protected DbSet<T> DbSet { get; }

    public Repository(DbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? await DbSet.CountAsync(cancellationToken)
            : await DbSet.CountAsync(predicate, cancellationToken);
    }

    public virtual void Add(T entity)
    {
        DbSet.Add(entity);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }
}
