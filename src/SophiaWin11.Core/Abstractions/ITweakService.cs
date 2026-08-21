namespace SophiaWin11.Core.Abstractions;

public interface ITweakService
{
    int TweakCount { get; }

    IReadOnlyList<ITweak> Tweaks { get; }

    Task InitializeCatalogAsync(CancellationToken cancellationToken = default);
}
