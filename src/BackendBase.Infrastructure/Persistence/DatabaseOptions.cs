namespace BackendBase.Infrastructure.Persistence;

/// <summary>
/// Strongly-typed database configuration, bound from the "Database" section of
/// appsettings. Selecting a real database is a config change, not a code change.
/// </summary>
public class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Which EF Core provider to use. See <see cref="DatabaseProvider"/>.</summary>
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.InMemory;

    /// <summary>Database name used by the InMemory provider only.</summary>
    public string InMemoryDatabaseName { get; set; } = "BackendBaseDb";
}

/// <summary>Supported EF Core providers.</summary>
public enum DatabaseProvider
{
    /// <summary>In-memory store. Zero setup, resets on every restart. Default for local/dev.</summary>
    InMemory = 0,

    /// <summary>Microsoft SQL Server. Uses ConnectionStrings:DefaultConnection.</summary>
    SqlServer = 1,

    /// <summary>PostgreSQL. Uses ConnectionStrings:DefaultConnection.</summary>
    PostgreSql = 2,
}
