namespace SophiaWin11.Core.Abstractions;

public sealed record TweakSession(
    Guid Id,
    DateTimeOffset CreatedAt,
    IReadOnlyList<ITweak> AppliedTweaks,
    bool RestorePointCreated,
    HealthDiagnosticResult? HealthDiagnostic);

public sealed class TweakConflictException : InvalidOperationException
{
    public TweakConflictException(IReadOnlyList<TweakConflict> conflicts)
        : base(BuildMessage(conflicts))
    {
        Conflicts = conflicts;
    }

    public IReadOnlyList<TweakConflict> Conflicts { get; }

    private static string BuildMessage(IReadOnlyList<TweakConflict> conflicts) =>
        $"{conflicts.Count} tweak conflict(s) detected: " +
        string.Join("; ", conflicts.Select(c => $"'{c.First.Name}' vs '{c.Second.Name}' ({c.Reason})"));
}

public interface ISessionService
{
    Task<TweakSession> ApplySessionAsync(IEnumerable<ITweak> tweaks, string description, CancellationToken cancellationToken = default);

    Task RollbackSessionAsync(TweakSession session, CancellationToken cancellationToken = default);
}
