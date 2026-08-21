using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Tweaks;
using SophiaWin11.Tests.Fakes;
using Xunit;

namespace SophiaWin11.Tests.Tweaks;

public sealed class DryRunPreviewTests
{
    private static readonly RegistryImpact Target = new(
        RegistryHive.CurrentUser,
        "Software\\SophiaWin11Tests",
        "TestValue",
        RegistryValueKind.DWord);

    [Fact]
    public async Task PreviewAsync_RegistryTweak_SeededValue_ShowsCurrentToTarget()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        virtualizer.Seed(Target.Hive, Target.SubKey, Target.ValueName, 0);
        var tweak = new RegistryTweak(
            Guid.NewGuid(), "Test", "TestTweak", "desc", Target, 1, 0, false, TweakRiskLevel.Low, virtualizer);

        var preview = await tweak.PreviewAsync();

        Assert.Contains("0", preview);
        Assert.Contains("1", preview);
        Assert.Contains("->", preview);
    }

    [Fact]
    public async Task PreviewAsync_RegistryTweak_NoExistingValue_ShowsNotSet()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var tweak = new RegistryTweak(
            Guid.NewGuid(), "Test", "TestTweak", "desc", Target, 1, 0, false, TweakRiskLevel.Low, virtualizer);

        var preview = await tweak.PreviewAsync();

        Assert.Contains("(not set)", preview);
    }

    [Fact]
    public async Task PreviewAsync_RegistryTweak_NeverCallsApply()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var tweak = new RegistryTweak(
            Guid.NewGuid(), "Test", "TestTweak", "desc", Target, 1, 0, false, TweakRiskLevel.Low, virtualizer);

        await tweak.PreviewAsync();

        Assert.Equal(0, virtualizer.WriteCount);
    }

    [Fact]
    public async Task PreviewAsync_PowerShellNativeTweak_ShowsApplyScriptWithoutExecuting()
    {
        var powerShellHost = new RecordingPowerShellHost();
        var tweak = new PowerShellNativeTweak(
            Guid.NewGuid(), "Test", "ScriptTweak", "desc",
            "Write-Output 'apply'", "Write-Output 'revert'", "",
            false, TweakRiskLevel.Low, powerShellHost);

        var preview = await tweak.PreviewAsync();

        Assert.Contains("Write-Output 'apply'", preview);
        Assert.Empty(powerShellHost.InvokedScripts);
    }
}
