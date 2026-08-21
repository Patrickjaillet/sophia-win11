using SophiaWin11.UI.Animation;
using Xunit;

namespace SophiaWin11.Tests.Animation;

public sealed class LoopClockTests
{
    [Fact]
    public void Advance_AccumulatesWithinDuration()
    {
        var result = LoopClock.Advance(TimeSpan.Zero, TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(2));

        Assert.Equal(TimeSpan.FromMilliseconds(500), result);
    }

    [Fact]
    public void Advance_WrapsAroundDuration()
    {
        var result = LoopClock.Advance(TimeSpan.FromMilliseconds(1900), TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(2000));

        Assert.Equal(TimeSpan.FromMilliseconds(200), result);
    }

    [Fact]
    public void Advance_ReturnsZero_WhenDurationIsZero()
    {
        var result = LoopClock.Advance(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50), TimeSpan.Zero);

        Assert.Equal(TimeSpan.Zero, result);
    }
}
