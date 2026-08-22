using System.Windows.Controls;
using SophiaWin11.App.ViewModels;

namespace SophiaWin11.App.Views;

public partial class SettingsPage : UserControl
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
