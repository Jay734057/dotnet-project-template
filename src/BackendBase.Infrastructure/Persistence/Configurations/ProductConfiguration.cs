using BackendBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendBase.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the <see cref="Product"/> entity to its table/columns. Keeping this in a
/// dedicated configuration class (rather than fluent calls inline in the
/// DbContext) is what lets the model scale to many entities cleanly.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        // Explicit precision so the column is money-safe on relational providers
        // (SQL Server / PostgreSQL) rather than defaulting to a lossy type.
        builder.Property(p => p.Price)
            .HasPrecision(18, 2);

        // Speeds up the name search this API exposes.
        builder.HasIndex(p => p.Name);
    }
}
