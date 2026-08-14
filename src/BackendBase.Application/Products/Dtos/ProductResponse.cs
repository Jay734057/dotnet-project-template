using BackendBase.Domain.Entities;

namespace BackendBase.Application.Products.Dtos;

/// <summary>
/// The API's outward-facing representation of a product. Kept separate from the
/// <see cref="Product"/> domain entity so persistence/domain changes don't leak
/// into the public contract that frontend developers depend on.
/// </summary>
public class ProductResponse
{
    /// <summary>Unique product identifier.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; init; }

    /// <summary>Product name.</summary>
    /// <example>Wireless Keyboard</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>Optional product description.</summary>
    /// <example>Compact mechanical keyboard with Bluetooth.</example>
    public string? Description { get; init; }

    /// <summary>Unit price in the catalog's base currency.</summary>
    /// <example>79.99</example>
    public decimal Price { get; init; }

    /// <summary>UTC timestamp the product was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>UTC timestamp the product was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>Projects a domain <see cref="Product"/> onto its API response shape.</summary>
    public static ProductResponse FromEntity(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt,
    };
}
