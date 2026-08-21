using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Extensions;
using Xunit;

namespace SophiaWin11.Tests.Extensions;

public sealed class ServiceCollectionExtensionsTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSophiaCore();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddSophiaCore_ResolvesTweakService()
    {
        using var provider = BuildProvider();
        var service = provider.GetRequiredService<ITweakService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void AddSophiaCore_ResolvesRegistryService()
    {
        using var provider = BuildProvider();
        var service = provider.GetRequiredService<IRegistryService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void AddSophiaCore_ResolvesElevationService()
    {
        using var provider = BuildProvider();
        var service = provider.GetRequiredService<IElevationService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void AddSophiaCore_ResolvesBackupService()
    {
        using var provider = BuildProvider();
        var service = provider.GetRequiredService<IBackupService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void AddSophiaCore_ResolvesLocalizationService()
    {
        using var provider = BuildProvider();
        var service = provider.GetRequiredService<ILocalizationService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void AddSophiaCore_ResolvesThemeService()
    {
        using var provider = BuildProvider();
        var service = provider.GetRequiredService<IThemeService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void AddSophiaCore_ResolvesAnimationService()
    {
        using var provider = BuildProvider();
        var service = provider.GetRequiredService<IAnimationService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void AddSophiaCore_AllServicesAreSingletons()
    {
        using var provider = BuildProvider();
        var first = provider.GetRequiredService<IThemeService>();
        var second = provider.GetRequiredService<IThemeService>();
        Assert.Same(first, second);
    }

    [Fact]
    public void ElevationService_IsRunningElevated_DoesNotThrowOnNonWindows()
    {
        using var provider = BuildProvider();
        var service = provider.GetRequiredService<IElevationService>();
        var exception = Record.Exception(() => service.IsRunningElevated);
        Assert.Null(exception);
    }

    [Fact]
    public void ThemeService_ApplyTheme_SetsIsThemeApplied()
    {
        using var provider = BuildProvider();
        var service = provider.GetRequiredService<IThemeService>();
        service.ApplyTheme();
        Assert.True(service.IsThemeApplied);
    }

    [Fact]
    public void AnimationService_AnimationsEnabled_DefaultsToTrue()
    {
        using var provider = BuildProvider();
        var service = provider.GetRequiredService<IAnimationService>();
        Assert.True(service.AnimationsEnabled);
    }

    [Fact]
    public void LocalizationService_SetCulture_UpdatesCurrentCulture()
    {
        using var provider = BuildProvider();
        var service = provider.GetRequiredService<ILocalizationService>();
        var culture = new System.Globalization.CultureInfo("fr-FR");
        service.SetCulture(culture);
        Assert.Equal(culture, service.CurrentCulture);
    }
}
