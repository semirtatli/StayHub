namespace StayHub.Shared.Pagination;

/// <summary>
/// Base class for paginated query requests.
/// All list queries should inherit from this to ensure consistent pagination.
/// </summary>
public abstract class PaginationParameters
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;
    private const int DefaultPage = 1;

    private int _page = DefaultPage;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// Page number (1-based). Defaults to 1.
    /// </summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? DefaultPage : value;
    }

    /// <summary>
    /// Number of items per page. Clamped between 1 and 100. Defaults to 10.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    /// <summary>
    /// Optional search term for filtering results.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Column name to sort by.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction: "asc" or "desc". Defaults to "asc".
    /// </summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>
    /// Whether sorting is descending.
    /// </summary>
    public bool IsDescending => SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
}
