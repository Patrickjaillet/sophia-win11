using System.Windows;
using System.Windows.Controls;
using SophiaWin11.App.ViewModels;
using SophiaWin11.UI.Animation;

namespace SophiaWin11.App.Views;

public partial class TweakRowView : UserControl
{
    public TweakRowView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var index = (DataContext as TweakRowViewModel)?.Index ?? 0;
        CardReveal.Play(this, index);
    }
}
