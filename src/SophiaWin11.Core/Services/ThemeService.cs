using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class ThemeService : IThemeService
{
    private readonly ILogger<ThemeService> _logger;

    public ThemeService(ILogger<ThemeService> logger)
    {
        _logger = logger;
    }

    public bool IsThemeApplied { get; private set; }

    public void ApplyTheme()
    {
        IsThemeApplied = true;
        _logger.LogInformation("Art Deco theme flagged as applied.");
    }
}
