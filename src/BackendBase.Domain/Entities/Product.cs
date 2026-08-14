using BackendBase.Domain.Common;

namespace BackendBase.Domain.Entities;

/// <summary>
/// Core domain entity representing a product in the catalog. Lives in the
/// Domain layer with no dependencies on EF Core, ASP.NET, or any other
/// outer-layer concern — persistence and transport shapes are defined
/// elsewhere (see the Infrastructure and Application layers).
/// </summary>
public class Product : IAuditable
{
    /// <summary>Unique identifier, assigned when the product is created.</summary>
    public Guid Id { get; set; }

    /// <summary>Human-readable product name. Required, searchable.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional free-text description.</summary>
    public string? Description { get; set; }

    /// <summary>Unit price in the catalog's base currency. Never negative.</summary>
    public decimal Price { get; set; }

    /// <summary>UTC timestamp set once when the product is created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>UTC timestamp updated on every modification.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
