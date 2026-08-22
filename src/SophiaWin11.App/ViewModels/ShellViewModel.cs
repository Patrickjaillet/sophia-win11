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
    public const string SettingsTag = "settings";
    public const string AboutTag = "about";

    private readonly ITweakService _tweakService;
    private readonly IElevationService _elevationService;
    private readonly ISnackbarService _snackbarService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;

    private NavigationViewItem? _activeItem;
    private string _activeTag = DashboardTag;

    [ObservableProperty]
    private bool isElevated;

    [ObservableProperty]
    private bool isInitializing = true;

    [ObservableProperty]
    private string statusText;

    public ShellViewModel(ITweakService tweakService, IElevationService elevationService, ISnackbarService snackbarService, ISessionService sessionService, ILocalizationService localizationService)
    {
        _tweakService = tweakService;
        _elevationService = elevationService;
        _snackbarService = snackbarService;
        _sessionService = sessionService;
        _localizationService = localizationService;

        IsElevated = _elevationService.IsRunningElevated;
        NavigationItems = new ObservableCollection<object>();
        statusText = _localizationService.GetString("Shell_StatusLoading");

        _localizationService.PropertyChanged += OnLocalizationPropertyChanged;
    }

    public event Action<string>? NavigationRequested;

    public ObservableCollection<object> NavigationItems { get; }

    private static readonly IReadOnlyList<string> CategoryIconKeysInCatalogOrder =
    [
        "IconGeometryPrivacyTelemetry",
        "IconGeometryUiPersonalization",
        "IconGeometrySystem",
        "IconGeometryGaming",
        "IconGeometryDefenderSecurity",
        "IconGeometryContextMenu",
    ];

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _tweakService.InitializeCatalogAsync(cancellationToken).ConfigureAwait(true);

            var categoryCount = _tweakService.Tweaks.Select(tweak => tweak.Category).Distinct().Count();
            BuildNavigationItems();

            StatusText = string.Format(_localizationService.GetString("Shell_StatusLoaded"), _tweakService.TweakCount, categoryCount);
            IsInitializing = false;

            if (NavigationItems.OfType<NavigationViewItem>().FirstOrDefault() is { } firstItem)
            {
                ActivateItem(firstItem, DashboardTag);
            }
        }
        catch (Exception ex)
        {
            StatusText = string.Format(_localizationService.GetString("Shell_StatusLoadFailed"), ex.Message);
            _snackbarService.Show(
                _localizationService.GetString("Shell_SnackbarStartupFailedTitle"),
                _localizationService.GetString("Shell_SnackbarStartupFailedMessage"),
                ControlAppearance.Danger,
                TimeSpan.FromSeconds(10));
        }
    }

    public const string CategoryTagPrefix = "cat:";

    public CategoryViewModel CreateCategoryViewModel(string tag)
    {
        var categoriesInCatalogOrder = _tweakService.Tweaks.Select(tweak => tweak.Category).Distinct().ToList();
        var index = int.Parse(tag[CategoryTagPrefix.Length..]);
        var category = categoriesInCatalogOrder[index];

        var tweaks = _tweakService.Tweaks.Where(tweak => tweak.Category == category).ToList();
        return new CategoryViewModel(category, tweaks, _snackbarService, _sessionService, _localizationService);
    }

    private async void OnLocalizationPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ILocalizationService.CurrentCulture))
        {
            return;
        }

        await _tweakService.InitializeCatalogAsync().ConfigureAwait(true);

        var previousTag = _activeTag;
        BuildNavigationItems();

        var itemToActivate = NavigationItems.OfType<NavigationViewItem>()
            .FirstOrDefault(item => Equals(item.Tag, previousTag));

        if (itemToActivate is not null)
        {
            ActivateItem(itemToActivate, previousTag);
        }

        LanguageChanged?.Invoke(previousTag);
    }

    public event Action<string>? LanguageChanged;

    private void BuildNavigationItems()
    {
        NavigationItems.Clear();

        var dashboardItem = new NavigationViewItem
        {
            Content = _localizationService.GetString("Nav_Dashboard"),
            Tag = DashboardTag,
            Icon = CreateIcon("IconGeometryDashboard"),
        };
        dashboardItem.Click += (_, _) => ActivateItem(dashboardItem, DashboardTag);
        NavigationItems.Add(dashboardItem);

        var searchItem = new NavigationViewItem
        {
            Content = _localizationService.GetString("Nav_Search"),
            Tag = SearchTag,
            Icon = CreateIcon("IconGeometrySearch"),
        };
        searchItem.Click += (_, _) => ActivateItem(searchItem, SearchTag);
        NavigationItems.Add(searchItem);

        var settingsItem = new NavigationViewItem
        {
            Content = _localizationService.GetString("Nav_Settings"),
            Tag = SettingsTag,
            Icon = CreateIcon("IconGeometrySystem"),
        };
        settingsItem.Click += (_, _) => ActivateItem(settingsItem, SettingsTag);
        NavigationItems.Add(settingsItem);

        var aboutItem = new NavigationViewItem
        {
            Content = _localizationService.GetString("Nav_About"),
            Tag = AboutTag,
            Icon = CreateIcon("IconGeometryAbout"),
        };
        aboutItem.Click += (_, _) => ActivateItem(aboutItem, AboutTag);
        NavigationItems.Add(aboutItem);

        var categoriesInCatalogOrder = _tweakService.Tweaks
            .Select(tweak => tweak.Category)
            .Distinct()
            .ToList();

        var iconKeyByCategory = new Dictionary<string, string>();
        for (var index = 0; index < categoriesInCatalogOrder.Count; index++)
        {
            iconKeyByCategory[categoriesInCatalogOrder[index]] = index < CategoryIconKeysInCatalogOrder.Count
                ? CategoryIconKeysInCatalogOrder[index]
                : "IconGeometrySystem";
        }

        var categories = categoriesInCatalogOrder.OrderBy(category => category, StringComparer.OrdinalIgnoreCase);

        foreach (var category in categories)
        {
            var categoryTag = CategoryTagPrefix + categoriesInCatalogOrder.IndexOf(category);
            var categoryItem = new NavigationViewItem
            {
                Content = category,
                Tag = categoryTag,
                Icon = CreateIcon(iconKeyByCategory[category]),
            };
            categoryItem.Click += (_, _) => ActivateItem(categoryItem, categoryTag);
            NavigationItems.Add(categoryItem);
        }
    }

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
        _activeTag = tag;

        NavigationRequested?.Invoke(tag);
    }
}
