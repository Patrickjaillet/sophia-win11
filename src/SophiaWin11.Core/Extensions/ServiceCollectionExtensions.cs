using Microsoft.Extensions.DependencyInjection;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Services;

namespace SophiaWin11.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSophiaCore(this IServiceCollection services)
    {
        services.AddSingleton<ITweakService, TweakService>();
        services.AddSingleton<IRegistryService, RegistryService>();
        services.AddSingleton<IElevationService, ElevationService>();
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IAnimationService, AnimationService>();

        return services;
    }
}
