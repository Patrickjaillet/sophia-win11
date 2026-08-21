using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using SophiaWin11.UI.Animation;
using SkottieAnimation = SkiaSharp.Skottie.Animation;

namespace SophiaWin11.UI.Controls;

public sealed class LottieAnimationPresenter : Grid
{
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source),
        typeof(Uri),
        typeof(LottieAnimationPresenter),
        new PropertyMetadata(null, OnSourceChanged));

    public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(
        nameof(IsPlaying),
        typeof(bool),
        typeof(LottieAnimationPresenter),
        new PropertyMetadata(true, OnIsPlayingChanged));

    private readonly SKElement _surface;
    private readonly RenderLoop _renderLoop;
    private SkottieAnimation? _animation;
    private TimeSpan _elapsed;
    private DateTime? _lastTick;

    public LottieAnimationPresenter()
    {
        _surface = new SKElement();
        _surface.PaintSurface += OnPaintSurface;
        Children.Add(_surface);

        _renderLoop = new RenderLoop(
            () => CompositionTarget.Rendering += OnRendering,
            () => CompositionTarget.Rendering -= OnRendering);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    [TypeConverter(typeof(UriTypeConverter))]
    public Uri? Source
    {
        get => (Uri?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((LottieAnimationPresenter)d).LoadAnimation((Uri?)e.NewValue);

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var presenter = (LottieAnimationPresenter)d;
        if ((bool)e.NewValue)
        {
            presenter.TryStart();
        }
        else
        {
            presenter._renderLoop.Stop();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (IsPlaying)
        {
            TryStart();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => _renderLoop.Stop();

    private void LoadAnimation(Uri? source)
    {
        _renderLoop.Stop();
        _animation?.Dispose();
        _animation = null;
        _elapsed = TimeSpan.Zero;

        if (source is not null)
        {
            var resourceInfo = Application.GetResourceStream(source)
                ?? throw new InvalidOperationException($"Lottie asset not found: {source}");

            using (resourceInfo.Stream)
            {
                _animation = SkottieAnimation.Create(resourceInfo.Stream);
            }

            _animation?.SeekFrameTime(TimeSpan.Zero);
        }

        _surface.InvalidateVisual();

        if (IsPlaying && IsLoaded)
        {
            TryStart();
        }
    }

    private void TryStart()
    {
        if (_animation is null)
        {
            return;
        }

        if (!MotionPolicy.AnimationsEnabled)
        {
            _animation.SeekFrameTime(TimeSpan.Zero);
            _surface.InvalidateVisual();
            return;
        }

        _lastTick = null;
        _renderLoop.Start();
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (_animation is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var delta = _lastTick is null ? TimeSpan.Zero : now - _lastTick.Value;
        _lastTick = now;

        _elapsed = LoopClock.Advance(_elapsed, delta, _animation.Duration);
        _animation.SeekFrameTime(_elapsed);
        _surface.InvalidateVisual();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (_animation is null)
        {
            return;
        }

        var info = e.Info;
        _animation.Render(canvas, new SKRect(0, 0, info.Width, info.Height));
    }
}
