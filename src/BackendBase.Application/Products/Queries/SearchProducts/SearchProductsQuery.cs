using BackendBase.Application.Common.Models;
using BackendBase.Application.Products.Dtos;
using MediatR;

namespace BackendBase.Application.Products.Queries.SearchProducts;

/// <summary>
/// Searches products by an optional name fragment, returning a paged, sorted
/// result set.
/// </summary>
/// <param name="Name">Case-insensitive substring to match against the product name. Null/empty returns all products.</param>
/// <param name="Page">1-based page number. Defaults to 1.</param>
/// <param name="PageSize">Items per page (1–100). Defaults to 20.</param>
/// <param name="SortBy">Field to sort by: <c>name</c>, <c>price</c>, or <c>createdAt</c>. Defaults to <c>name</c>.</param>
/// <param name="Descending">Sort descending when true. Defaults to false (ascending).</param>
public record SearchProductsQuery(
    string? Name = null,
    int Page = SearchProductsQuery.DefaultPage,
    int PageSize = SearchProductsQuery.DefaultPageSize,
    string SortBy = SearchProductsQuery.DefaultSortBy,
    bool Descending = false) : IRequest<PagedResult<ProductResponse>>
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const string DefaultSortBy = "name";

    /// <summary>Fields the API allows sorting by. Guards against arbitrary property injection.</summary>
    public static readonly IReadOnlyCollection<string> AllowedSortFields =
        new[] { "name", "price", "createdat" };
}
