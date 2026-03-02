using Microsoft.EntityFrameworkCore;
using StayHub.Shared.Domain;
using StayHub.Shared.Infrastructure.Specifications;
using StayHub.Shared.Pagination;

namespace StayHub.Shared.Infrastructure.Persistence;

/// <summary>
/// Extended repository that supports the Specification pattern.
/// Service-specific repositories inherit from this instead of Repository&lt;T&gt;
/// when they need specification-based queries.
///
/// Usage:
///   public class HotelRepository : SpecificationRepository&lt;Hotel&gt;, IHotelRepository
///   {
///       public HotelRepository(HotelDbContext context) : base(context) { }
///   }
///
/// Then in application layer:
///   var spec = new TopRatedHotelsSpec(city, minStars);
///   var hotels = await _hotelRepo.ListAsync(spec);
/// </summary>
public class SpecificationRepository<T> : Repository<T> where T : AggregateRoot
{
    public SpecificationRepository(DbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Returns a single entity matching the specification, or null.
    /// </summary>
    public virtual async Task<T?> FirstOrDefaultAsync(
        Specification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Returns all entities matching the specification.
    /// </summary>
    public virtual async Task<IReadOnlyList<T>> ListAsync(
        Specification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Returns a paginated list of entities matching the specification.
    /// The specification should NOT include Skip/Take — pagination is applied here.
    /// </summary>
    public virtual async Task<PagedList<T>> PagedListAsync(
        Specification<T> specification,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedList<T>.Create(items, page, pageSize, totalCount);
    }

    /// <summary>
    /// Returns the count of entities matching the specification.
    /// </summary>
    public virtual async Task<int> CountAsync(
        Specification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification)
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Returns whether any entity matches the specification.
    /// </summary>
    public virtual async Task<bool> AnyAsync(
        Specification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification)
            .AnyAsync(cancellationToken);
    }

    private IQueryable<T> ApplySpecification(Specification<T> specification)
    {
        return SpecificationEvaluator.GetQuery(DbSet.AsQueryable(), specification);
    }
}
