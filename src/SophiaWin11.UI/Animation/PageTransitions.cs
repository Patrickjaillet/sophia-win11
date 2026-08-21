using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SophiaWin11.UI.Animation;

public static class PageTransitions
{
    private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(200);
    private const double EnterOffset = 14;

    public static void Enter(FrameworkElement element)
    {
        if (!MotionPolicy.AnimationsEnabled)
        {
            element.Opacity = 1;
            return;
        }

        var transform = new TranslateTransform(0, EnterOffset);
        element.RenderTransform = transform;
        element.Opacity = 0;

        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        var opacityAnimation = new DoubleAnimation(0, 1, Duration) { EasingFunction = ease };
        var translateAnimation = new DoubleAnimation(EnterOffset, 0, Duration) { EasingFunction = ease };

        transform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
        element.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
    }
}
