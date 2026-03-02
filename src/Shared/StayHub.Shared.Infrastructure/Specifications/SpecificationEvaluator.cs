using Microsoft.EntityFrameworkCore;
using StayHub.Shared.Domain;

namespace StayHub.Shared.Infrastructure.Specifications;

/// <summary>
/// Translates a Specification into an EF Core IQueryable.
/// Used by SpecificationRepository to evaluate specs against the database.
///
/// Applies each specification property in order:
/// 1. Criteria (WHERE)
/// 2. Includes (JOIN)
/// 3. OrderBy/OrderByDescending
/// 4. Paging (Skip/Take)
/// 5. Tracking/Split options
/// </summary>
public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(
        IQueryable<T> inputQuery,
        Specification<T> specification) where T : Entity
    {
        var query = inputQuery;

        // Apply WHERE criteria
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply eager loading (expression-based includes)
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based includes (for nested ThenInclude)
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply pagination
        if (specification.Skip.HasValue)
        {
            query = query.Skip(specification.Skip.Value);
        }

        if (specification.Take.HasValue)
        {
            query = query.Take(specification.Take.Value);
        }

        // Apply tracking option
        if (specification.IsNoTracking)
        {
            query = query.AsNoTracking();
        }

        // Apply split query option
        if (specification.IsSplitQuery)
        {
            query = query.AsSplitQuery();
        }

        return query;
    }
}
