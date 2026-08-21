using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class TweakService : ITweakService
{
    private readonly ILogger<TweakService> _logger;

    public TweakService(ILogger<TweakService> logger)
    {
        _logger = logger;
    }

    public int TweakCount { get; private set; }

    public Task InitializeCatalogAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
