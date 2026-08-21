using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Tweaks;
using SophiaWin11.Tests.Fakes;
using Xunit;

namespace SophiaWin11.Tests.Tweaks;

public sealed class RegistryTweakTests
{
    private static readonly RegistryImpact Target = new(
        RegistryHive.CurrentUser,
        "Software\\SophiaWin11Tests",
        "TestValue",
        RegistryValueKind.DWord);

    [Fact]
    public async Task ApplyAsync_WritesApplyValue()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var tweak = new RegistryTweak(
            Guid.NewGuid(), "Test", "TestTweak", "desc", Target, 1, 0, false, TweakRiskLevel.Low, virtualizer);

        await tweak.ApplyAsync();

        Assert.Equal(1, virtualizer.GetValue(Target.Hive, Target.SubKey, Target.ValueName));
    }

    [Fact]
    public async Task RevertAsync_WithRevertValue_RestoresPreviousValue()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        virtualizer.Seed(Target.Hive, Target.SubKey, Target.ValueName, 0);
        var tweak = new RegistryTweak(
            Guid.NewGuid(), "Test", "TestTweak", "desc", Target, 1, 0, false, TweakRiskLevel.Low, virtualizer);

        await tweak.ApplyAsync();
        await tweak.RevertAsync();

        Assert.Equal(0, virtualizer.GetValue(Target.Hive, Target.SubKey, Target.ValueName));
    }

    [Fact]
    public async Task RevertAsync_WithNullRevertValue_DeletesValue()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var tweak = new RegistryTweak(
            Guid.NewGuid(), "Test", "TestTweak", "desc", Target, 1, null, false, TweakRiskLevel.Low, virtualizer);

        await tweak.ApplyAsync();
        Assert.NotNull(virtualizer.GetValue(Target.Hive, Target.SubKey, Target.ValueName));

        await tweak.RevertAsync();

        Assert.Null(virtualizer.GetValue(Target.Hive, Target.SubKey, Target.ValueName));
    }

    [Fact]
    public async Task IsAppliedAsync_ReturnsTrue_WhenCurrentValueMatchesApplyValue()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var tweak = new RegistryTweak(
            Guid.NewGuid(), "Test", "TestTweak", "desc", Target, 1, 0, false, TweakRiskLevel.Low, virtualizer);

        await tweak.ApplyAsync();

        Assert.True(await tweak.IsAppliedAsync());
    }

    [Fact]
    public async Task IsAppliedAsync_ReturnsFalse_AfterRevert()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var tweak = new RegistryTweak(
            Guid.NewGuid(), "Test", "TestTweak", "desc", Target, 1, 0, false, TweakRiskLevel.Low, virtualizer);

        await tweak.ApplyAsync();
        await tweak.RevertAsync();

        Assert.False(await tweak.IsAppliedAsync());
    }

    [Fact]
    public async Task ApplyAsync_NeverTouchesRealRegistry_OnlyVirtualizer()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var tweak = new RegistryTweak(
            Guid.NewGuid(), "Test", "TestTweak", "desc", Target, 1, 0, false, TweakRiskLevel.Low, virtualizer);

        await tweak.ApplyAsync();

        Assert.Equal(1, virtualizer.WriteCount);
        Assert.Equal(0, virtualizer.DeleteCount);
    }

    [Fact]
    public async Task ApplyAsync_MediumRisk_TriggersSnapshotCapture()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var snapshotService = new RecordingSnapshotService();
        var tweak = new RegistryTweak(
            Guid.NewGuid(), "Test", "TestTweak", "desc", Target, 1, 0, false, TweakRiskLevel.Medium, virtualizer, snapshotService);

        await tweak.ApplyAsync();

        Assert.Equal(1, snapshotService.CaptureCount);
    }

    [Fact]
    public async Task ApplyAsync_LowRisk_DoesNotTriggerSnapshotCapture()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var snapshotService = new RecordingSnapshotService();
        var tweak = new RegistryTweak(
            Guid.NewGuid(), "Test", "TestTweak", "desc", Target, 1, 0, false, TweakRiskLevel.Low, virtualizer, snapshotService);

        await tweak.ApplyAsync();

        Assert.Equal(0, snapshotService.CaptureCount);
    }

    private sealed class RecordingSnapshotService : ITweakSnapshotService
    {
        public int CaptureCount { get; private set; }

        public Task<string> CaptureAsync(ITweak tweak, CancellationToken cancellationToken = default)
        {
            CaptureCount++;
            return Task.FromResult("in-memory-snapshot");
        }

        public Task<IReadOnlyList<RegistryValueSnapshot>> ReadAsync(string snapshotPath, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<RegistryValueSnapshot>>([]);
    }
}
