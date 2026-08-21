using System.Reflection;
using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Catalog;

namespace SophiaWin11.Core.Services;

public sealed class TweakService : ITweakService
{
    private const string EmbeddedCatalogResourceName = "SophiaWin11.Core.Catalog.tweaks.en.json";

    private readonly ILogger<TweakService> _logger;
    private readonly IRegistryService _registryService;
    private readonly IPowerShellHost _powerShellHost;
    private readonly IWin32InteropHost _win32InteropHost;
    private readonly ITweakSnapshotService _snapshotService;
    private IReadOnlyList<ITweak> _catalog = [];

    public TweakService(
        ILogger<TweakService> logger,
        IRegistryService registryService,
        IPowerShellHost powerShellHost,
        IWin32InteropHost win32InteropHost,
        ITweakSnapshotService snapshotService)
    {
        _logger = logger;
        _registryService = registryService;
        _powerShellHost = powerShellHost;
        _win32InteropHost = win32InteropHost;
        _snapshotService = snapshotService;
    }

    public int TweakCount => _catalog.Count;

    public Task InitializeCatalogAsync(CancellationToken cancellationToken = default)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(EmbeddedCatalogResourceName)
                            ?? throw new InvalidOperationException($"Embedded catalog resource '{EmbeddedCatalogResourceName}' not found.");
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var loader = new TweakCatalogLoader(_registryService, _powerShellHost, _win32InteropHost, _snapshotService);
        _catalog = loader.LoadFromJson(json);

        _logger.LogInformation("Tweak catalog initialized with {Count} tweaks.", _catalog.Count);

        return Task.CompletedTask;
    }
}
