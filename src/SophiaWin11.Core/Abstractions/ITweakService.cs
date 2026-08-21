namespace SophiaWin11.Core.Abstractions;

public interface ITweakService
{
    int TweakCount { get; }

    Task InitializeCatalogAsync(CancellationToken cancellationToken = default);
}
