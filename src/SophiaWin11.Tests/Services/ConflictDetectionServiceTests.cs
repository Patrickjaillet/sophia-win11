using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Services;
using SophiaWin11.Core.Tweaks;
using SophiaWin11.Tests.Fakes;
using Xunit;

namespace SophiaWin11.Tests.Services;

public sealed class ConflictDetectionServiceTests
{
    private static readonly RegistryImpact SharedTarget = new(
        RegistryHive.LocalMachine,
        "Software\\Policies\\Microsoft\\Windows\\Test",
        "DohSetting",
        RegistryValueKind.DWord);

    [Fact]
    public void DetectConflicts_TweaksTargetingSameValueWithDifferentApplyValues_ReturnsConflict()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var first = new RegistryTweak(
            Guid.NewGuid(), "Test", "CloudflareDoH", "desc", SharedTarget, 1, 0, false, TweakRiskLevel.Low, virtualizer);
        var second = new RegistryTweak(
            Guid.NewGuid(), "Test", "GoogleDoH", "desc", SharedTarget, 2, 0, false, TweakRiskLevel.Low, virtualizer);

        var service = new ConflictDetectionService();
        var conflicts = service.DetectConflicts([first, second]);

        var conflict = Assert.Single(conflicts);
        Assert.Equal(first.Id, conflict.First.Id);
        Assert.Equal(second.Id, conflict.Second.Id);
        Assert.Contains("different values", conflict.Reason);
    }

    [Fact]
    public void DetectConflicts_TweaksTargetingSameValueWithSameApplyValue_ReturnsNoConflict()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var first = new RegistryTweak(
            Guid.NewGuid(), "Test", "TweakA", "desc", SharedTarget, 1, 0, false, TweakRiskLevel.Low, virtualizer);
        var second = new RegistryTweak(
            Guid.NewGuid(), "Test", "TweakB", "desc", SharedTarget, 1, 0, false, TweakRiskLevel.Low, virtualizer);

        var service = new ConflictDetectionService();
        var conflicts = service.DetectConflicts([first, second]);

        Assert.Empty(conflicts);
    }

    [Fact]
    public void DetectConflicts_TweaksTargetingDifferentValues_ReturnsNoConflict()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var otherTarget = SharedTarget with { ValueName = "UnrelatedSetting" };
        var first = new RegistryTweak(
            Guid.NewGuid(), "Test", "TweakA", "desc", SharedTarget, 1, 0, false, TweakRiskLevel.Low, virtualizer);
        var second = new RegistryTweak(
            Guid.NewGuid(), "Test", "TweakB", "desc", otherTarget, 1, 0, false, TweakRiskLevel.Low, virtualizer);

        var service = new ConflictDetectionService();
        var conflicts = service.DetectConflicts([first, second]);

        Assert.Empty(conflicts);
    }

    [Fact]
    public void DetectConflicts_TweaksTargetingSameValueWithEqualByteArrays_ReturnsNoConflict()
    {
        var binaryTarget = SharedTarget with { ValueKind = RegistryValueKind.Binary };
        var virtualizer = new RegistryHiveVirtualizer();
        var first = new RegistryTweak(
            Guid.NewGuid(), "Test", "TweakA", "desc", binaryTarget, new byte[] { 0x01, 0x02 }, null, false, TweakRiskLevel.Low, virtualizer);
        var second = new RegistryTweak(
            Guid.NewGuid(), "Test", "TweakB", "desc", binaryTarget, new byte[] { 0x01, 0x02 }, null, false, TweakRiskLevel.Low, virtualizer);

        var service = new ConflictDetectionService();
        var conflicts = service.DetectConflicts([first, second]);

        Assert.Empty(conflicts);
    }

    [Fact]
    public void DetectConflicts_NoTweaksSelected_ReturnsEmpty()
    {
        var service = new ConflictDetectionService();
        var conflicts = service.DetectConflicts([]);

        Assert.Empty(conflicts);
    }
}
