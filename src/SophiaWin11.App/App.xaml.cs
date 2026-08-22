using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SophiaWin11.App.Localization;
using SophiaWin11.App.ViewModels;
using SophiaWin11.App.Views;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Extensions;
using Wpf.Ui;

namespace SophiaWin11.App;

public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var logPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SophiaWin11", "logs", "sophia-.log");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(ConfigureServices)
            .Build();

        _host.Start();

        var localizationService = _host.Services.GetRequiredService<ILocalizationService>();
        localizationService.InitializeAsync().GetAwaiter().GetResult();
        TranslateExtension.LocalizationService = localizationService;

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSophiaCore();

        services.AddSingleton<ISnackbarService, SnackbarService>();
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<MainWindow>();

        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DashboardPage>();
        services.AddTransient<SearchViewModel>();
        services.AddTransient<SearchPage>();
        services.AddTransient<CategoryPage>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<AboutViewModel>();
        services.AddTransient<AboutPage>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.StopAsync().GetAwaiter().GetResult();
        _host?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
