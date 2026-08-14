using BackendBase.Application.Common.Interfaces;
using BackendBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendBase.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IProductRepository"/>. This is the only
/// place product queries are expressed against the database; the filtering,
/// sorting, and paging are all pushed down to the provider rather than done in
/// memory.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(
        string? nameFilter,
        int page,
        int pageSize,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var term = nameFilter.Trim().ToLower();
            // ToLower().Contains(...) translates to a case-insensitive LIKE on
            // relational providers and also works on the InMemory provider.
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySort(query, sortBy, descending);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken) =>
        await _dbContext.Products.AddAsync(product, cancellationToken);

    public void Update(Product product) => _dbContext.Products.Update(product);

    public void Remove(Product product) => _dbContext.Products.Remove(product);

    private static IQueryable<Product> ApplySort(IQueryable<Product> query, string? sortBy, bool descending) =>
        (sortBy?.ToLowerInvariant()) switch
        {
            "price" => descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "createdat" => descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
        };
}
