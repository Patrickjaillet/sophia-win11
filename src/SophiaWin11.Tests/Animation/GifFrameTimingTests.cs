using SophiaWin11.UI.Animation;
using Xunit;

namespace SophiaWin11.Tests.Animation;

public sealed class GifFrameTimingTests
{
    private static readonly IReadOnlyList<TimeSpan> ThreeEqualFrames =
    [
        TimeSpan.FromMilliseconds(100),
        TimeSpan.FromMilliseconds(100),
        TimeSpan.FromMilliseconds(100),
    ];

    [Fact]
    public void ResolveFrameIndex_ReturnsNegativeOne_WhenNoFrames()
    {
        Assert.Equal(-1, GifFrameTiming.ResolveFrameIndex(Array.Empty<TimeSpan>(), TimeSpan.Zero));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(150, 1)]
    [InlineData(250, 2)]
    [InlineData(320, 0)]
    public void ResolveFrameIndex_CyclesThroughFramesAndWraps(double elapsedMilliseconds, int expectedIndex)
    {
        var elapsed = TimeSpan.FromMilliseconds(elapsedMilliseconds);

        var index = GifFrameTiming.ResolveFrameIndex(ThreeEqualFrames, elapsed);

        Assert.Equal(expectedIndex, index);
    }

    [Fact]
    public void ResolveFrameIndex_ClampsNegativeElapsedToFirstFrame()
    {
        var index = GifFrameTiming.ResolveFrameIndex(ThreeEqualFrames, TimeSpan.FromMilliseconds(-50));

        Assert.Equal(0, index);
    }

    [Fact]
    public void TotalDuration_SumsAllFrameDurations()
    {
        var total = GifFrameTiming.TotalDuration(ThreeEqualFrames);

        Assert.Equal(TimeSpan.FromMilliseconds(300), total);
    }
}
