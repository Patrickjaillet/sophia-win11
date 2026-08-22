using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class LocalizationService : ILocalizationService
{
    private static readonly IReadOnlyList<CultureInfo> SupportedCultureList =
    [
        CultureInfo.GetCultureInfo("en"),
        CultureInfo.GetCultureInfo("fr"),
        CultureInfo.GetCultureInfo("de"),
        CultureInfo.GetCultureInfo("ru"),
        CultureInfo.GetCultureInfo("uk"),
    ];

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly ILogger<LocalizationService> _logger;
    private readonly ResourceManager _resourceManager;
    private readonly string _settingsFilePath;
    private CultureInfo _currentCulture = CultureInfo.GetCultureInfo("en");

    public LocalizationService(ILogger<LocalizationService> logger)
    {
        _logger = logger;
        _resourceManager = new ResourceManager("SophiaWin11.Core.Localization.Strings", typeof(LocalizationService).Assembly);
        _settingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SophiaWin11",
            "settings.json");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public CultureInfo CurrentCulture => _currentCulture;

    public IReadOnlyList<CultureInfo> SupportedCultures => SupportedCultureList;

    public string this[string key] => GetString(key);

    public string GetString(string key)
    {
        try
        {
            var value = _resourceManager.GetString(key, _currentCulture);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve localization key '{Key}' for culture '{Culture}'.", key, _currentCulture.Name);
        }

        _logger.LogWarning("Localization key '{Key}' is missing for culture '{Culture}'; falling back to English.", key, _currentCulture.Name);

        try
        {
            var fallback = _resourceManager.GetString(key, CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(fallback))
            {
                return fallback;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve fallback localization key '{Key}'.", key);
        }

        return key;
    }

    public void SetCulture(CultureInfo culture)
    {
        var resolved = ResolveSupportedCulture(culture);

        if (Equals(_currentCulture, resolved))
        {
            return;
        }

        _currentCulture = resolved;

        PersistPreference(resolved);

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var persisted = await LoadPersistedCultureAsync(cancellationToken).ConfigureAwait(false);

        _currentCulture = persisted ?? DetectSystemCulture();
    }

    private CultureInfo DetectSystemCulture()
    {
        var systemCulture = CultureInfo.InstalledUICulture;
        var resolved = SupportedCultureList.FirstOrDefault(culture =>
            string.Equals(culture.TwoLetterISOLanguageName, systemCulture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase));

        return resolved ?? CultureInfo.GetCultureInfo("en");
    }

    private static CultureInfo ResolveSupportedCulture(CultureInfo culture)
    {
        var isSupported = SupportedCultureList.Any(supported =>
            string.Equals(supported.TwoLetterISOLanguageName, culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase));

        return isSupported ? culture : CultureInfo.GetCultureInfo("en");
    }

    private async Task<CultureInfo?> LoadPersistedCultureAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(_settingsFilePath, cancellationToken).ConfigureAwait(false);
            var settings = JsonSerializer.Deserialize<LocalizationSettings>(json, SerializerOptions);

            if (settings is null || string.IsNullOrWhiteSpace(settings.Language))
            {
                return null;
            }

            return ResolveSupportedCulture(CultureInfo.GetCultureInfo(settings.Language));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load persisted language preference from '{Path}'.", _settingsFilePath);
            return null;
        }
    }

    private void PersistPreference(CultureInfo culture)
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var settings = new LocalizationSettings(culture.TwoLetterISOLanguageName);
            var json = JsonSerializer.Serialize(settings, SerializerOptions);
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist language preference to '{Path}'.", _settingsFilePath);
        }
    }

    private sealed record LocalizationSettings(string Language);
}
