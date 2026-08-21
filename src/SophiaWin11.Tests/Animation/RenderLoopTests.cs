using SophiaWin11.UI.Animation;
using Xunit;

namespace SophiaWin11.Tests.Animation;

public sealed class RenderLoopTests
{
    [Fact]
    public void Start_SubscribesExactlyOnce_WhenCalledRepeatedly()
    {
        var subscribeCount = 0;
        var loop = new RenderLoop(() => subscribeCount++, () => { });

        loop.Start();
        loop.Start();
        loop.Start();

        Assert.Equal(1, subscribeCount);
        Assert.True(loop.IsRunning);
    }

    [Fact]
    public void Stop_UnsubscribesExactlyOnce_WhenCalledRepeatedly()
    {
        var unsubscribeCount = 0;
        var loop = new RenderLoop(() => { }, () => unsubscribeCount++);

        loop.Start();
        loop.Stop();
        loop.Stop();
        loop.Stop();

        Assert.Equal(1, unsubscribeCount);
        Assert.False(loop.IsRunning);
    }

    [Fact]
    public void Stop_WithoutStart_DoesNotUnsubscribe()
    {
        var unsubscribeCount = 0;
        var loop = new RenderLoop(() => { }, () => unsubscribeCount++);

        loop.Stop();

        Assert.Equal(0, unsubscribeCount);
    }

    [Fact]
    public void Start_AfterStop_ReSubscribes()
    {
        var subscribeCount = 0;
        var loop = new RenderLoop(() => subscribeCount++, () => { });

        loop.Start();
        loop.Stop();
        loop.Start();

        Assert.Equal(2, subscribeCount);
    }
}
