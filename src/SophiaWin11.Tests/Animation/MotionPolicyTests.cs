using SophiaWin11.UI.Animation;
using Xunit;

namespace SophiaWin11.Tests.Animation;

public sealed class MotionPolicyTests
{
    [Fact]
    public void GetEffectiveDuration_ReturnsZero_WhenAnimationsDisabled()
    {
        var requested = TimeSpan.FromMilliseconds(250);

        var effective = MotionPolicy.GetEffectiveDuration(requested, animationsEnabled: false);

        Assert.Equal(TimeSpan.Zero, effective);
    }

    [Fact]
    public void GetEffectiveDuration_ReturnsRequestedDuration_WhenAnimationsEnabled()
    {
        var requested = TimeSpan.FromMilliseconds(250);

        var effective = MotionPolicy.GetEffectiveDuration(requested, animationsEnabled: true);

        Assert.Equal(requested, effective);
    }

    [Fact]
    public void AnimationsEnabled_ReflectsLiveSystemParameter()
    {
        Assert.Equal(System.Windows.SystemParameters.ClientAreaAnimation, MotionPolicy.AnimationsEnabled);
    }
}
