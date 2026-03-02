namespace StayHub.Shared.Pagination;

/// <summary>
/// Represents a paginated list of items with metadata for cursor-less pagination.
/// Used as the standard response for all list endpoints.
/// </summary>
/// <typeparam name="T">The type of items in the paginated list.</typeparam>
public sealed class PagedList<T>
{
    /// <summary>
    /// The items in the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    private PagedList(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    /// <summary>
    /// Creates a paged list from an already-materialized collection.
    /// Use this when you've already executed the query and have the items in memory.
    /// </summary>
    public static PagedList<T> Create(
        IReadOnlyList<T> items,
        int page,
        int pageSize,
        int totalCount)
    {
        return new PagedList<T>(items, page, pageSize, totalCount);
    }

    /// <summary>
    /// Maps the items in this paged list to a different type,
    /// preserving the pagination metadata.
    /// </summary>
    public PagedList<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        var mappedItems = Items.Select(selector).ToList().AsReadOnly();
        return PagedList<TResult>.Create(mappedItems, Page, PageSize, TotalCount);
    }

    /// <summary>
    /// Creates an empty paged list.
    /// </summary>
    public static PagedList<T> Empty(int page = 1, int pageSize = 10)
    {
        return new PagedList<T>(Array.Empty<T>(), page, pageSize, 0);
    }
}
