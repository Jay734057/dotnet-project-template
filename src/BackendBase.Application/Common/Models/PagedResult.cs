namespace BackendBase.Application.Common.Models;

/// <summary>
/// A single page of results plus the paging metadata a client needs to render
/// pagination controls. Returned by search/list queries.
/// </summary>
/// <typeparam name="T">The type of item in the page.</typeparam>
public class PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    /// <summary>The items on this page.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>The 1-based page number these items came from.</summary>
    public int Page { get; }

    /// <summary>The maximum number of items per page.</summary>
    public int PageSize { get; }

    /// <summary>Total number of items matching the query, across all pages.</summary>
    public int TotalCount { get; }

    /// <summary>Total number of pages available for the current page size.</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Whether a page after this one exists.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Whether a page before this one exists.</summary>
    public bool HasPreviousPage => Page > 1;
}
