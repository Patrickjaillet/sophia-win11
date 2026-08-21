using System.Windows.Controls;
using SophiaWin11.App.ViewModels;

namespace SophiaWin11.App.Views;

public partial class SearchPage : UserControl
{
    public SearchPage(SearchViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
