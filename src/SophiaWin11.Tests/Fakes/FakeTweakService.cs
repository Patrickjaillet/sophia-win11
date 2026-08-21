using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Tests.Fakes;

public sealed class FakeTweakService : ITweakService
{
    public FakeTweakService(IReadOnlyList<ITweak> tweaks)
    {
        Tweaks = tweaks;
    }

    public int TweakCount => Tweaks.Count;

    public IReadOnlyList<ITweak> Tweaks { get; }

    public Task InitializeCatalogAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
