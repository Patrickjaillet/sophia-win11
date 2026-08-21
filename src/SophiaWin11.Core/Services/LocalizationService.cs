using System.Globalization;
using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class LocalizationService : ILocalizationService
{
    private readonly ILogger<LocalizationService> _logger;
    private CultureInfo _currentCulture = CultureInfo.InvariantCulture;

    public LocalizationService(ILogger<LocalizationService> logger)
    {
        _logger = logger;
    }

    public CultureInfo CurrentCulture => _currentCulture;

    public string GetString(string key)
    {
        throw new NotImplementedException();
    }

    public void SetCulture(CultureInfo culture)
    {
        _currentCulture = culture;
    }
}
