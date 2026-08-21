using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SophiaWin11.UI.Animation;

public static class CardReveal
{
    private static readonly TimeSpan BaseDuration = TimeSpan.FromMilliseconds(260);
    private static readonly TimeSpan StaggerStep = TimeSpan.FromMilliseconds(35);
    private static readonly TimeSpan MaxStagger = TimeSpan.FromMilliseconds(350);
    private const double StartScale = 0.92;

    public static void Play(FrameworkElement element, int index)
    {
        if (!MotionPolicy.AnimationsEnabled)
        {
            element.Opacity = 1;
            return;
        }

        var stagger = ResolveStagger(index);

        var scale = new ScaleTransform(StartScale, StartScale);
        element.RenderTransform = scale;
        element.RenderTransformOrigin = new Point(0.5, 0.5);
        element.Opacity = 0;

        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        var opacityAnimation = new DoubleAnimation(0, 1, BaseDuration) { BeginTime = stagger, EasingFunction = ease };
        var scaleXAnimation = new DoubleAnimation(StartScale, 1, BaseDuration) { BeginTime = stagger, EasingFunction = ease };
        var scaleYAnimation = new DoubleAnimation(StartScale, 1, BaseDuration) { BeginTime = stagger, EasingFunction = ease };

        element.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
    }

    public static TimeSpan ResolveStagger(int index)
    {
        var clampedIndex = Math.Max(index, 0);
        var requested = TimeSpan.FromTicks(StaggerStep.Ticks * clampedIndex);
        return requested > MaxStagger ? MaxStagger : requested;
    }
}
