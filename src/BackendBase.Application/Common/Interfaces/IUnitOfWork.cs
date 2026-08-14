namespace BackendBase.Application.Common.Interfaces;

/// <summary>
/// Commits all staged changes across repositories as a single atomic unit.
/// Kept separate from the repositories on purpose: a handler that touches
/// several repositories in one request still commits exactly once, by calling
/// <see cref="SaveChangesAsync"/> a single time.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes and returns the number of affected rows.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
