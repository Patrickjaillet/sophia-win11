using System.Windows.Controls;
using SophiaWin11.App.ViewModels;

namespace SophiaWin11.App.Views;

public partial class AboutPage : UserControl
{
    public AboutPage(AboutViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
