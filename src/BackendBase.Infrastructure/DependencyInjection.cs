using BackendBase.Application.Common.Interfaces;
using BackendBase.Infrastructure.Persistence;
using BackendBase.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BackendBase.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer. Wires up the DbContext for the
/// configured provider plus the repository/unit-of-work implementations, so
/// nothing above this layer depends on EF Core.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();

        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        // Abstracted clock so audit timestamps are deterministic under test.
        services.AddSingleton(TimeProvider.System);

        services.AddDbContext<AppDbContext>(options =>
            ConfigureProvider(options, databaseOptions, configuration));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static void ConfigureProvider(
        DbContextOptionsBuilder options,
        DatabaseOptions databaseOptions,
        IConfiguration configuration)
    {
        switch (databaseOptions.Provider)
        {
            case DatabaseProvider.SqlServer:
                options.UseSqlServer(RequireConnectionString(configuration, databaseOptions.Provider));
                break;

            case DatabaseProvider.PostgreSql:
                options.UseNpgsql(RequireConnectionString(configuration, databaseOptions.Provider));
                break;

            case DatabaseProvider.InMemory:
            default:
                // Ships as the default so the API runs with zero external setup.
                // Data resets on every restart — that is expected, not a bug.
                options.UseInMemoryDatabase(databaseOptions.InMemoryDatabaseName);
                break;
        }
    }

    private static string RequireConnectionString(IConfiguration configuration, DatabaseProvider provider)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Database provider '{provider}' requires ConnectionStrings:DefaultConnection to be set. " +
                "Provide it via user-secrets, environment variables, or a secrets manager.");
        }

        return connectionString;
    }
}
