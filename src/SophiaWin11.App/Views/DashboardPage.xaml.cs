using System.Windows.Controls;
using SophiaWin11.App.ViewModels;

namespace SophiaWin11.App.Views;

public partial class DashboardPage : UserControl
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
