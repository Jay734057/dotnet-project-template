using BackendBase.Domain.Entities;

namespace BackendBase.Application.Common.Interfaces;

/// <summary>
/// Abstraction over product persistence. Defined in the Application layer and
/// implemented in Infrastructure, so handlers depend only on this contract and
/// never on EF Core directly — that is what keeps them unit-testable and the
/// database provider swappable.
/// </summary>
public interface IProductRepository
{
    /// <summary>Fetch a single product by id, or <c>null</c> if it does not exist.</summary>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Search products by (optional) name fragment, returning a single page of
    /// results together with the total count matching the filter (before
    /// paging) so callers can compute page counts.
    /// </summary>
    /// <param name="nameFilter">Case-insensitive substring to match against the name; null/empty matches all.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="sortBy">Field to sort by: "name", "price", or "createdAt".</param>
    /// <param name="descending">Whether to sort in descending order.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(
        string? nameFilter,
        int page,
        int pageSize,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken);

    /// <summary>Stage a new product for insertion. Persisted on the next unit-of-work commit.</summary>
    Task AddAsync(Product product, CancellationToken cancellationToken);

    /// <summary>Stage an existing product's changes. Persisted on the next unit-of-work commit.</summary>
    void Update(Product product);

    /// <summary>Stage a product for deletion. Persisted on the next unit-of-work commit.</summary>
    void Remove(Product product);
}
