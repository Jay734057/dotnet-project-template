using BackendBase.Application.Common.Interfaces;
using BackendBase.Application.Common.Models;
using BackendBase.Application.Products.Dtos;
using MediatR;

namespace BackendBase.Application.Products.Queries.SearchProducts;

/// <summary>
/// Handles <see cref="SearchProductsQuery"/> by delegating filtering, sorting,
/// and paging to the repository (which pushes them down to the database) and
/// projecting the page onto response DTOs.
/// </summary>
public class SearchProductsQueryHandler
    : IRequestHandler<SearchProductsQuery, PagedResult<ProductResponse>>
{
    private readonly IProductRepository _repository;

    public SearchProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ProductResponse>> Handle(
        SearchProductsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.SearchAsync(
            request.Name,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.Descending,
            cancellationToken);

        var responses = items.Select(ProductResponse.FromEntity).ToList();

        return new PagedResult<ProductResponse>(responses, request.Page, request.PageSize, totalCount);
    }
}
