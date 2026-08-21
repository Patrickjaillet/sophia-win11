using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.App.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    private readonly IElevationService _elevationService;

    [ObservableProperty]
    private string statusText = "Sophia Script for Win11 — v0.2.0.0";

    [ObservableProperty]
    private bool isElevated;

    public MainViewModel(IThemeService themeService, IElevationService elevationService)
    {
        _themeService = themeService;
        _elevationService = elevationService;

        IsElevated = _elevationService.IsRunningElevated;
    }

    [RelayCommand]
    private void ApplyTheme()
    {
        _themeService.ApplyTheme();
        StatusText = _themeService.IsThemeApplied
            ? "Theme applied"
            : "Theme not applied";
    }
}
