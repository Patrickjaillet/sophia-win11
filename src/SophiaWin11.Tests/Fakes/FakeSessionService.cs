using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Tests.Fakes;

public sealed class FakeSessionService : ISessionService
{
    public List<IReadOnlyList<ITweak>> ApplyCalls { get; } = [];

    public List<TweakSession> RollbackCalls { get; } = [];

    public IReadOnlyList<TweakConflict> ConflictsToThrow { get; set; } = [];

    public async Task<TweakSession> ApplySessionAsync(
        IEnumerable<ITweak> tweaks,
        string description,
        CancellationToken cancellationToken = default)
    {
        var selection = tweaks.ToList();

        if (ConflictsToThrow.Count > 0)
        {
            throw new TweakConflictException(ConflictsToThrow);
        }

        ApplyCalls.Add(selection);

        foreach (var tweak in selection)
        {
            await tweak.ApplyAsync(cancellationToken).ConfigureAwait(false);
        }

        return new TweakSession(Guid.NewGuid(), DateTimeOffset.UtcNow, selection, false, null);
    }

    public async Task RollbackSessionAsync(TweakSession session, CancellationToken cancellationToken = default)
    {
        RollbackCalls.Add(session);

        for (var i = session.AppliedTweaks.Count - 1; i >= 0; i--)
        {
            await session.AppliedTweaks[i].RevertAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
