using System.Linq.Expressions;
using StayHub.Shared.Domain;

namespace StayHub.Shared.Interfaces;

/// <summary>
/// Generic repository interface for aggregate roots.
/// Only aggregate roots can be persisted — child entities are saved through their aggregate.
/// Infrastructure implements this with EF Core.
/// </summary>
public interface IRepository<T> where T : AggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    void Add(T entity);

    void Update(T entity);

    void Remove(T entity);
}
