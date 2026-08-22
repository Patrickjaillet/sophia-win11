using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Catalog;

namespace SophiaWin11.Core.Services;

public sealed class TweakService : ITweakService
{
    private static readonly IReadOnlyCollection<string> SupportedCatalogLanguages = ["fr", "de", "ru", "uk"];

    private readonly ILogger<TweakService> _logger;
    private readonly IRegistryService _registryService;
    private readonly IPowerShellHost _powerShellHost;
    private readonly IWin32InteropHost _win32InteropHost;
    private readonly ITweakSnapshotService _snapshotService;
    private readonly ILocalizationService _localizationService;
    private IReadOnlyList<ITweak> _catalog = [];

    public TweakService(
        ILogger<TweakService> logger,
        IRegistryService registryService,
        IPowerShellHost powerShellHost,
        IWin32InteropHost win32InteropHost,
        ITweakSnapshotService snapshotService,
        ILocalizationService localizationService)
    {
        _logger = logger;
        _registryService = registryService;
        _powerShellHost = powerShellHost;
        _win32InteropHost = win32InteropHost;
        _snapshotService = snapshotService;
        _localizationService = localizationService;
    }

    public int TweakCount => _catalog.Count;

    public IReadOnlyList<ITweak> Tweaks => _catalog;

    public Task InitializeCatalogAsync(CancellationToken cancellationToken = default)
    {
        var resourceName = ResolveCatalogResourceName(_localizationService.CurrentCulture);

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)
                            ?? throw new InvalidOperationException($"Embedded catalog resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var loader = new TweakCatalogLoader(_registryService, _powerShellHost, _win32InteropHost, _snapshotService);
        _catalog = loader.LoadFromJson(json);

        _logger.LogInformation("Tweak catalog initialized with {Count} tweaks from '{Resource}'.", _catalog.Count, resourceName);

        return Task.CompletedTask;
    }

    private static string ResolveCatalogResourceName(CultureInfo culture)
    {
        var language = culture.TwoLetterISOLanguageName;

        return SupportedCatalogLanguages.Contains(language)
            ? $"SophiaWin11.Core.Catalog.tweaks.{language}.json"
            : "SophiaWin11.Core.Catalog.tweaks.en.json";
    }
}
