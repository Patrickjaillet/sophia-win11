using SophiaWin11.UI.Animation;
using Xunit;

namespace SophiaWin11.Tests.Animation;

public sealed class CardRevealTests
{
    [Fact]
    public void ResolveStagger_ScalesLinearlyWithIndex()
    {
        var stagger0 = CardReveal.ResolveStagger(0);
        var stagger1 = CardReveal.ResolveStagger(1);
        var stagger2 = CardReveal.ResolveStagger(2);

        Assert.Equal(TimeSpan.Zero, stagger0);
        Assert.True(stagger1 > stagger0);
        Assert.Equal(TimeSpan.FromTicks(stagger1.Ticks * 2), stagger2);
    }

    [Fact]
    public void ResolveStagger_IsCappedAtMaximum()
    {
        var farStagger = CardReveal.ResolveStagger(1000);
        var cappedStagger = CardReveal.ResolveStagger(50);

        Assert.Equal(farStagger, cappedStagger);
    }

    [Fact]
    public void ResolveStagger_ClampsNegativeIndexToZero()
    {
        Assert.Equal(TimeSpan.Zero, CardReveal.ResolveStagger(-5));
    }
}
