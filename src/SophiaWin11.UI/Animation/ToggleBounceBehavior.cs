using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SophiaWin11.UI.Animation;

public static class ToggleBounceBehavior
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(ToggleBounceBehavior),
        new PropertyMetadata(false, OnIsEnabledChanged));

    private static readonly TimeSpan OvershootTime = TimeSpan.FromMilliseconds(90);
    private static readonly TimeSpan SettleTime = TimeSpan.FromMilliseconds(220);
    private const double OvershootScale = 1.18;

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ToggleButton toggle)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            toggle.RenderTransform = new ScaleTransform(1, 1);
            toggle.RenderTransformOrigin = new Point(0.5, 0.5);
            toggle.Checked += OnToggled;
            toggle.Unchecked += OnToggled;
        }
        else
        {
            toggle.Checked -= OnToggled;
            toggle.Unchecked -= OnToggled;
        }
    }

    private static void OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { RenderTransform: ScaleTransform scale })
        {
            return;
        }

        if (!MotionPolicy.AnimationsEnabled)
        {
            return;
        }

        var ease = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.6 };

        var scaleXAnimation = new DoubleAnimationUsingKeyFrames();
        scaleXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(OvershootScale, KeyTime.FromTimeSpan(OvershootTime), ease));
        scaleXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(SettleTime), ease));

        var scaleYAnimation = (DoubleAnimationUsingKeyFrames)scaleXAnimation.Clone();

        scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
    }
}
