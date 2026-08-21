using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SophiaWin11.UI.Animation;

namespace SophiaWin11.UI.Controls;

public sealed class AnimatedGifPresenter : Image
{
    public static readonly DependencyProperty GifSourceProperty = DependencyProperty.Register(
        nameof(GifSource),
        typeof(Uri),
        typeof(AnimatedGifPresenter),
        new PropertyMetadata(null, OnGifSourceChanged));

    public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(
        nameof(IsPlaying),
        typeof(bool),
        typeof(AnimatedGifPresenter),
        new PropertyMetadata(true, OnIsPlayingChanged));

    public static readonly DependencyProperty LoopProperty = DependencyProperty.Register(
        nameof(Loop),
        typeof(bool),
        typeof(AnimatedGifPresenter),
        new PropertyMetadata(true));

    private static readonly Dictionary<Uri, FrameSet> FrameCache = new();

    private readonly RenderLoop _renderLoop;
    private FrameSet? _frames;
    private TimeSpan _elapsed;
    private int _currentIndex = -1;
    private DateTime? _lastTick;

    public AnimatedGifPresenter()
    {
        _renderLoop = new RenderLoop(
            () => CompositionTarget.Rendering += OnRendering,
            () => CompositionTarget.Rendering -= OnRendering);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    [TypeConverter(typeof(UriTypeConverter))]
    public Uri? GifSource
    {
        get => (Uri?)GetValue(GifSourceProperty);
        set => SetValue(GifSourceProperty, value);
    }

    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public bool Loop
    {
        get => (bool)GetValue(LoopProperty);
        set => SetValue(LoopProperty, value);
    }

    private static void OnGifSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => _ = ((AnimatedGifPresenter)d).LoadFramesAsync((Uri?)e.NewValue);

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var presenter = (AnimatedGifPresenter)d;
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

    private async Task LoadFramesAsync(Uri? source)
    {
        _renderLoop.Stop();
        _frames = null;
        _elapsed = TimeSpan.Zero;
        _currentIndex = -1;
        Source = null;

        if (source is null)
        {
            return;
        }

        if (!FrameCache.TryGetValue(source, out var frameSet))
        {
            frameSet = await Task.Run(() => DecodeFrames(source)).ConfigureAwait(true);
            FrameCache[source] = frameSet;
        }

        _frames = frameSet;

        if (_frames.Images.Count > 0)
        {
            ApplyFrame(0);

            if (IsPlaying && IsLoaded)
            {
                TryStart();
            }
        }
    }

    private static FrameSet DecodeFrames(Uri source)
    {
        var resourceInfo = Application.GetResourceStream(source)
            ?? throw new InvalidOperationException($"Gif asset not found: {source}");

        using var stream = resourceInfo.Stream;
        var decoder = new GifBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

        var images = new List<BitmapSource>(decoder.Frames.Count);
        var durations = new List<TimeSpan>(decoder.Frames.Count);

        foreach (var frame in decoder.Frames)
        {
            frame.Freeze();
            images.Add(frame);
            durations.Add(ReadFrameDelay(frame));
        }

        return new FrameSet(images, durations);
    }

    private static TimeSpan ReadFrameDelay(BitmapFrame frame)
    {
        if (frame.Metadata is BitmapMetadata metadata)
        {
            try
            {
                if (metadata.GetQuery("/grctlext/Delay") is ushort hundredths)
                {
                    return TimeSpan.FromMilliseconds(hundredths == 0 ? 100 : hundredths * 10);
                }
            }
            catch (NotSupportedException)
            {
            }
            catch (IOException)
            {
            }
        }

        return TimeSpan.FromMilliseconds(100);
    }

    private void TryStart()
    {
        if (_frames is null || _frames.Images.Count <= 1 || !MotionPolicy.AnimationsEnabled)
        {
            return;
        }

        _renderLoop.Start();
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (_frames is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var delta = _lastTick is null ? TimeSpan.Zero : now - _lastTick.Value;
        _lastTick = now;

        _elapsed += delta;

        var totalDuration = GifFrameTiming.TotalDuration(_frames.Durations);
        if (!Loop && _elapsed >= totalDuration)
        {
            ApplyFrame(_frames.Images.Count - 1);
            _renderLoop.Stop();
            return;
        }

        var index = GifFrameTiming.ResolveFrameIndex(_frames.Durations, _elapsed);
        ApplyFrame(index);
    }

    private void ApplyFrame(int index)
    {
        if (_frames is null || index < 0 || index >= _frames.Images.Count || index == _currentIndex)
        {
            return;
        }

        _currentIndex = index;
        Source = _frames.Images[index];
    }

    private sealed class FrameSet
    {
        public FrameSet(IReadOnlyList<BitmapSource> images, IReadOnlyList<TimeSpan> durations)
        {
            Images = images;
            Durations = durations;
        }

        public IReadOnlyList<BitmapSource> Images { get; }

        public IReadOnlyList<TimeSpan> Durations { get; }
    }
}
