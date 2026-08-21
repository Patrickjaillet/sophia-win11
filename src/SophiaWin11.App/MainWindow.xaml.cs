using Microsoft.Extensions.DependencyInjection;
using SophiaWin11.App.ViewModels;
using SophiaWin11.App.Views;
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
            RootNavigation.ReplaceContent(_serviceProvider.GetRequiredService<DashboardPage>());
            return;
        }

        if (tag == ShellViewModel.SearchTag)
        {
            RootNavigation.ReplaceContent(_serviceProvider.GetRequiredService<SearchPage>());
            return;
        }

        var categoryViewModel = _viewModel.CreateCategoryViewModel(tag);
        var categoryPage = _serviceProvider.GetRequiredService<CategoryPage>();
        RootNavigation.ReplaceContent(categoryPage, categoryViewModel);
        _ = categoryViewModel.LoadAsync();
    }
}
