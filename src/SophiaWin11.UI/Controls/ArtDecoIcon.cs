using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace SophiaWin11.UI.Controls;

public sealed class ArtDecoIcon : IconElement
{
    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
        nameof(Data),
        typeof(Geometry),
        typeof(ArtDecoIcon),
        new PropertyMetadata(null));

    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    protected override UIElement InitializeChildren()
    {
        var path = new Path
        {
            Stretch = Stretch.Uniform,
            SnapsToDevicePixels = true,
        };

        path.SetBinding(Path.DataProperty, new System.Windows.Data.Binding(nameof(Data)) { Source = this });
        path.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding(nameof(Foreground)) { Source = this });
        path.SetValue(Path.StrokeThicknessProperty, 1.4);
        path.SetValue(Path.StrokeLineJoinProperty, PenLineJoin.Round);

        return new Viewbox
        {
            Width = 24,
            Height = 24,
            Child = path,
        };
    }
}
