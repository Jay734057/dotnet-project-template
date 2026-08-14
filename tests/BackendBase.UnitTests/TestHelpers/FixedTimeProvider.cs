namespace BackendBase.UnitTests.TestHelpers;

/// <summary>
/// A <see cref="TimeProvider"/> that always returns a fixed instant, so tests
/// that assert on audit timestamps are deterministic.
/// </summary>
public sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _now;

    public FixedTimeProvider(DateTimeOffset now) => _now = now;

    public override DateTimeOffset GetUtcNow() => _now;
}
