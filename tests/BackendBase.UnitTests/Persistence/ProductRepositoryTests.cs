using BackendBase.Domain.Entities;
using BackendBase.Infrastructure.Persistence;
using BackendBase.Infrastructure.Persistence.Repositories;
using BackendBase.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BackendBase.UnitTests.Persistence;

/// <summary>
/// Exercises <see cref="ProductRepository"/> against the EF Core InMemory
/// provider so filtering, sorting, paging, and the DbContext's audit-timestamp
/// behavior are all covered end to end without an external database.
/// </summary>
public class ProductRepositoryTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static AppDbContext NewContext() =>
        new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"products-{Guid.NewGuid()}")
                .Options,
            new FixedTimeProvider(FixedNow));

    private static async Task SeedAsync(AppDbContext ctx, params string[] names)
    {
        foreach (var name in names)
        {
            ctx.Products.Add(new Product { Id = Guid.NewGuid(), Name = name, Price = name.Length });
        }

        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task SaveChanges_stamps_audit_timestamps_on_insert()
    {
        await using var ctx = NewContext();
        var product = new Product { Id = Guid.NewGuid(), Name = "Widget", Price = 1m };
        ctx.Products.Add(product);

        await ctx.SaveChangesAsync();

        product.CreatedAt.Should().Be(FixedNow);
        product.UpdatedAt.Should().Be(FixedNow);
    }

    [Fact]
    public async Task SearchAsync_filters_by_name_case_insensitively()
    {
        await using var ctx = NewContext();
        await SeedAsync(ctx, "Apple", "Applesauce", "Banana");
        var repo = new ProductRepository(ctx);

        var (items, total) = await repo.SearchAsync("apple", 1, 10, "name", false, CancellationToken.None);

        total.Should().Be(2);
        items.Select(p => p.Name).Should().BeEquivalentTo("Apple", "Applesauce");
    }

    [Fact]
    public async Task SearchAsync_sorts_by_name_descending()
    {
        await using var ctx = NewContext();
        await SeedAsync(ctx, "Alpha", "Bravo", "Charlie");
        var repo = new ProductRepository(ctx);

        var (items, _) = await repo.SearchAsync(null, 1, 10, "name", descending: true, CancellationToken.None);

        items.Select(p => p.Name).Should().ContainInOrder("Charlie", "Bravo", "Alpha");
    }

    [Fact]
    public async Task SearchAsync_pages_results_and_reports_total()
    {
        await using var ctx = NewContext();
        await SeedAsync(ctx, "P1", "P2", "P3", "P4", "P5");
        var repo = new ProductRepository(ctx);

        var (items, total) = await repo.SearchAsync(null, 2, 2, "name", false, CancellationToken.None);

        total.Should().Be(5);
        items.Should().HaveCount(2);
        items.Select(p => p.Name).Should().ContainInOrder("P3", "P4");
    }
}
