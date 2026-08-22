using Microsoft.Extensions.DependencyInjection;
using SophiaWin11.App.ViewModels;
using SophiaWin11.App.Views;
using SophiaWin11.UI.Animation;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SophiaWin11.App;

public partial class MainWindow : FluentWindow
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ShellViewModel _viewModel;

    public MainWindow(ShellViewModel viewModel, IServiceProvider serviceProvider, ISnackbarService snackbarService)
    {
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;

        InitializeComponent();

        DataContext = _viewModel;

        snackbarService.SetSnackbarPresenter(RootSnackbarPresenter);
        _viewModel.NavigationRequested += OnNavigationRequested;
        _viewModel.LanguageChanged += OnNavigationRequested;

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }

    private void OnNavigationRequested(string tag)
    {
        if (tag == ShellViewModel.DashboardTag)
        {
            var page = _serviceProvider.GetRequiredService<DashboardPage>();
            RootNavigation.ReplaceContent(page);
            PageTransitions.Enter(page);
            return;
        }

        if (tag == ShellViewModel.SearchTag)
        {
            var page = _serviceProvider.GetRequiredService<SearchPage>();
            RootNavigation.ReplaceContent(page);
            PageTransitions.Enter(page);
            return;
        }

        if (tag == ShellViewModel.SettingsTag)
        {
            var page = _serviceProvider.GetRequiredService<SettingsPage>();
            RootNavigation.ReplaceContent(page);
            PageTransitions.Enter(page);
            return;
        }

        var categoryViewModel = _viewModel.CreateCategoryViewModel(tag);
        var categoryPage = _serviceProvider.GetRequiredService<CategoryPage>();
        RootNavigation.ReplaceContent(categoryPage, categoryViewModel);
        PageTransitions.Enter(categoryPage);
        _ = categoryViewModel.LoadAsync();
    }
}
