using BackendBase.Domain.Common;
using BackendBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendBase.Infrastructure.Persistence;

/// <summary>
/// The application's EF Core database context. Entity-to-table mapping lives in
/// per-entity <c>IEntityTypeConfiguration</c> classes (see the Configurations
/// folder) rather than inline here, so the context stays small as entities are
/// added. Audit timestamps for <see cref="IAuditable"/> entities are stamped
/// centrally in the SaveChanges overrides.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly TimeProvider _timeProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, TimeProvider timeProvider)
        : base(options)
    {
        _timeProvider = timeProvider;
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up every IEntityTypeConfiguration in this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    private void ApplyAuditTimestamps()
    {
        var now = _timeProvider.GetUtcNow();

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    // Never let a client's payload rewrite the original creation time.
                    entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;
                    break;
            }
        }
    }
}
