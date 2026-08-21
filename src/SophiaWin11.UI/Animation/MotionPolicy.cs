using System.ComponentModel;
using System.Windows;

namespace SophiaWin11.UI.Animation;

public static class MotionPolicy
{
    static MotionPolicy()
    {
        SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;
    }

    public static event EventHandler? AnimationsEnabledChanged;

    public static bool AnimationsEnabled => SystemParameters.ClientAreaAnimation;

    public static TimeSpan GetEffectiveDuration(TimeSpan requestedDuration, bool animationsEnabled)
        => animationsEnabled ? requestedDuration : TimeSpan.Zero;

    private static void OnSystemParametersChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemParameters.ClientAreaAnimation))
        {
            AnimationsEnabledChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
