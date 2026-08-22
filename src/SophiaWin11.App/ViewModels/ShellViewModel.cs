using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.UI.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SophiaWin11.App.ViewModels;

public sealed partial class ShellViewModel : ObservableObject
{
    public const string DashboardTag = "dashboard";
    public const string SearchTag = "search";

    private readonly ITweakService _tweakService;
    private readonly IElevationService _elevationService;
    private readonly ISnackbarService _snackbarService;
    private readonly ISessionService _sessionService;

    private NavigationViewItem? _activeItem;

    [ObservableProperty]
    private bool isElevated;

    [ObservableProperty]
    private bool isInitializing = true;

    [ObservableProperty]
    private string statusText = "Loading tweak catalog...";

    public ShellViewModel(ITweakService tweakService, IElevationService elevationService, ISnackbarService snackbarService, ISessionService sessionService)
    {
        _tweakService = tweakService;
        _elevationService = elevationService;
        _snackbarService = snackbarService;
        _sessionService = sessionService;

        IsElevated = _elevationService.IsRunningElevated;
        NavigationItems = new ObservableCollection<object>();
    }

    public event Action<string>? NavigationRequested;

    public ObservableCollection<object> NavigationItems { get; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _tweakService.InitializeCatalogAsync(cancellationToken).ConfigureAwait(true);

            var categoryCount = _tweakService.Tweaks.Select(tweak => tweak.Category).Distinct().Count();
            BuildNavigationItems();

            StatusText = $"{_tweakService.TweakCount} tweaks loaded across {categoryCount} categories.";
            IsInitializing = false;

            if (NavigationItems.OfType<NavigationViewItem>().FirstOrDefault() is { } firstItem)
            {
                ActivateItem(firstItem, DashboardTag);
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to load the tweak catalog: {ex.Message}";
            _snackbarService.Show(
                "Startup failed",
                "The tweak catalog could not be loaded. See the log for details.",
                ControlAppearance.Danger,
                TimeSpan.FromSeconds(10));
        }
    }

    public CategoryViewModel CreateCategoryViewModel(string category)
    {
        var tweaks = _tweakService.Tweaks.Where(tweak => tweak.Category == category).ToList();
        return new CategoryViewModel(category, tweaks, _snackbarService, _sessionService);
    }

    private void BuildNavigationItems()
    {
        NavigationItems.Clear();

        var dashboardItem = new NavigationViewItem
        {
            Content = "Dashboard",
            Tag = DashboardTag,
            Icon = CreateIcon("IconGeometryDashboard"),
        };
        dashboardItem.Click += (_, _) => ActivateItem(dashboardItem, DashboardTag);
        NavigationItems.Add(dashboardItem);

        var searchItem = new NavigationViewItem
        {
            Content = "Search",
            Tag = SearchTag,
            Icon = CreateIcon("IconGeometrySearch"),
        };
        searchItem.Click += (_, _) => ActivateItem(searchItem, SearchTag);
        NavigationItems.Add(searchItem);

        var categories = _tweakService.Tweaks
            .Select(tweak => tweak.Category)
            .Distinct()
            .OrderBy(category => category, StringComparer.OrdinalIgnoreCase);

        foreach (var category in categories)
        {
            var categoryItem = new NavigationViewItem
            {
                Content = category,
                Tag = category,
                Icon = CreateIcon(ResolveCategoryIconKey(category)),
            };
            categoryItem.Click += (_, _) => ActivateItem(categoryItem, category);
            NavigationItems.Add(categoryItem);
        }
    }

    private static string ResolveCategoryIconKey(string category) => category switch
    {
        "Privacy & Telemetry" => "IconGeometryPrivacyTelemetry",
        "UI & Personalization" => "IconGeometryUiPersonalization",
        "System" => "IconGeometrySystem",
        "Gaming" => "IconGeometryGaming",
        "Microsoft Defender & Security" => "IconGeometryDefenderSecurity",
        "Context menu" => "IconGeometryContextMenu",
        _ => "IconGeometrySystem",
    };

    private static ArtDecoIcon CreateIcon(string geometryResourceKey)
    {
        var geometry = Application.Current.TryFindResource(geometryResourceKey) as Geometry;
        return new ArtDecoIcon { Data = geometry };
    }

    private void ActivateItem(NavigationViewItem item, string tag)
    {
        if (_activeItem is not null)
        {
            _activeItem.IsActive = false;
        }

        item.IsActive = true;
        _activeItem = item;

        NavigationRequested?.Invoke(tag);
    }
}
