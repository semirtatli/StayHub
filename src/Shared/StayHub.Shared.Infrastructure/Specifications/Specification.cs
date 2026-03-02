using System.Linq.Expressions;
using StayHub.Shared.Domain;

namespace StayHub.Shared.Infrastructure.Specifications;

/// <summary>
/// Specification pattern — encapsulates query criteria, includes, and ordering
/// into a reusable, composable, testable object.
///
/// Instead of:
///   var hotels = await _context.Hotels
///       .Where(h => h.City == "Istanbul" &amp;&amp; h.StarRating >= 4)
///       .Include(h => h.Rooms)
///       .OrderByDescending(h => h.Rating)
///       .ToListAsync();
///
/// You write:
///   var spec = new TopRatedHotelsInCitySpec("Istanbul", minStars: 4);
///   var hotels = await _repository.ListAsync(spec);
///
/// Benefits:
/// - Domain logic stays in the domain/application layer (not scattered in repository)
/// - Specifications can be unit-tested without a database
/// - Complex queries are named and reusable
/// </summary>
public abstract class Specification<T> where T : Entity
{
    /// <summary>
    /// The WHERE clause.
    /// </summary>
    public Expression<Func<T, bool>>? Criteria { get; protected init; }

    /// <summary>
    /// Navigation properties to eagerly load (Include).
    /// </summary>
    public List<Expression<Func<T, object>>> Includes { get; } = [];

    /// <summary>
    /// String-based includes for nested properties (ThenInclude).
    /// Example: "Rooms.RoomAmenities"
    /// </summary>
    public List<string> IncludeStrings { get; } = [];

    /// <summary>
    /// ORDER BY ascending expression.
    /// </summary>
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    /// <summary>
    /// ORDER BY descending expression.
    /// </summary>
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    /// <summary>
    /// Number of records to skip (for pagination).
    /// </summary>
    public int? Skip { get; private set; }

    /// <summary>
    /// Number of records to take (for pagination).
    /// </summary>
    public int? Take { get; private set; }

    /// <summary>
    /// Whether to use .AsNoTracking() for read-only queries.
    /// </summary>
    public bool IsNoTracking { get; private set; }

    /// <summary>
    /// Whether to use .AsSplitQuery() for multiple includes.
    /// Prevents cartesian explosion when loading multiple collections.
    /// </summary>
    public bool IsSplitQuery { get; private set; }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    protected void ApplyNoTracking()
    {
        IsNoTracking = true;
    }

    protected void ApplySplitQuery()
    {
        IsSplitQuery = true;
    }
}
