using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SophiaWin11.UI.Animation;

public static class LoopingOrnament
{
    private static readonly TimeSpan RotationDuration = TimeSpan.FromSeconds(6);

    public static void Start(FrameworkElement element)
    {
        if (!MotionPolicy.AnimationsEnabled)
        {
            return;
        }

        var rotateTransform = new RotateTransform();
        element.RenderTransform = rotateTransform;
        element.RenderTransformOrigin = new Point(0.5, 0.5);

        var animation = new DoubleAnimation(0, 360, RotationDuration)
        {
            RepeatBehavior = RepeatBehavior.Forever,
        };

        rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
    }

    public static void Stop(FrameworkElement element)
    {
        if (element.RenderTransform is RotateTransform rotateTransform)
        {
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
        }
    }
}
