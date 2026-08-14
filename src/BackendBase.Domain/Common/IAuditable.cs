namespace BackendBase.Domain.Common;

/// <summary>
/// Marks an entity whose creation and modification timestamps are maintained
/// automatically by the persistence layer (see the DbContext's SaveChanges
/// override). Handlers never set these by hand, which keeps auditing consistent
/// across every entity that opts in.
/// </summary>
public interface IAuditable
{
    /// <summary>UTC timestamp set once when the entity is first persisted.</summary>
    DateTimeOffset CreatedAt { get; set; }

    /// <summary>UTC timestamp updated on every persisted modification.</summary>
    DateTimeOffset UpdatedAt { get; set; }
}
