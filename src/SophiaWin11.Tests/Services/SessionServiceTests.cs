using Microsoft.Extensions.Logging.Abstractions;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Services;
using SophiaWin11.Core.Tweaks;
using SophiaWin11.Tests.Fakes;
using Xunit;

namespace SophiaWin11.Tests.Services;

public sealed class SessionServiceTests
{
    private sealed class FakeRestorePointService : IRestorePointService
    {
        public int CallCount { get; private set; }

        public bool ReturnValue { get; set; } = true;

        public Task<bool> CreateRestorePointAsync(string description, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(ReturnValue);
        }
    }

    private sealed class FakeHealthDiagnosticService : IHealthDiagnosticService
    {
        public int CallCount { get; private set; }

        public bool Healthy { get; set; } = true;

        public Task<HealthDiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(new HealthDiagnosticResult(Healthy, Healthy ? "ok" : "not ok"));
        }
    }

    private static SessionService CreateService(
        FakeRestorePointService restorePointService,
        FakeHealthDiagnosticService healthDiagnosticService) =>
        new(
            new ConflictDetectionService(),
            restorePointService,
            healthDiagnosticService,
            NullLogger<SessionService>.Instance);

    [Fact]
    public async Task ApplySessionAsync_ThenRollback_RestoresRegistryToInitialStateBitForBit()
    {
        var virtualizer = new RegistryHiveVirtualizer();

        var targetA = new RegistryImpact(RegistryHive.CurrentUser, "Software\\SophiaWin11Tests\\A", "Value", RegistryValueKind.DWord);
        var targetB = new RegistryImpact(RegistryHive.CurrentUser, "Software\\SophiaWin11Tests\\B", "Value", RegistryValueKind.String);
        var targetC = new RegistryImpact(RegistryHive.LocalMachine, "Software\\SophiaWin11Tests\\C", "Value", RegistryValueKind.DWord);

        virtualizer.Seed(targetA.Hive, targetA.SubKey, targetA.ValueName, 0);
        virtualizer.Seed(targetB.Hive, targetB.SubKey, targetB.ValueName, "initial");
        virtualizer.Seed(targetC.Hive, targetC.SubKey, targetC.ValueName, 5);

        var tweakA = new RegistryTweak(Guid.NewGuid(), "Test", "TweakA", "desc", targetA, 1, 0, false, TweakRiskLevel.Low, virtualizer);
        var tweakB = new RegistryTweak(Guid.NewGuid(), "Test", "TweakB", "desc", targetB, "changed", "initial", false, TweakRiskLevel.Medium, virtualizer);
        var tweakC = new RegistryTweak(Guid.NewGuid(), "Test", "TweakC", "desc", targetC, 42, 5, false, TweakRiskLevel.Low, virtualizer);

        var restorePointService = new FakeRestorePointService();
        var healthDiagnosticService = new FakeHealthDiagnosticService();
        var service = CreateService(restorePointService, healthDiagnosticService);

        var session = await service.ApplySessionAsync([tweakA, tweakB, tweakC], "Test session");

        Assert.Equal(1, restorePointService.CallCount);
        Assert.Equal(1, virtualizer.GetValue(targetA.Hive, targetA.SubKey, targetA.ValueName));
        Assert.Equal("changed", virtualizer.GetValue(targetB.Hive, targetB.SubKey, targetB.ValueName));
        Assert.Equal(42, virtualizer.GetValue(targetC.Hive, targetC.SubKey, targetC.ValueName));
        Assert.Equal(3, session.AppliedTweaks.Count);

        await service.RollbackSessionAsync(session);

        Assert.Equal(0, virtualizer.GetValue(targetA.Hive, targetA.SubKey, targetA.ValueName));
        Assert.Equal("initial", virtualizer.GetValue(targetB.Hive, targetB.SubKey, targetB.ValueName));
        Assert.Equal(5, virtualizer.GetValue(targetC.Hive, targetC.SubKey, targetC.ValueName));
    }

    [Fact]
    public async Task ApplySessionAsync_CreatesExactlyOneRestorePoint_ForTheWholeSession_WhenSessionContainsMediumRisk()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var target1 = new RegistryImpact(RegistryHive.CurrentUser, "Software\\SophiaWin11Tests\\X", "Value", RegistryValueKind.DWord);
        var target2 = new RegistryImpact(RegistryHive.CurrentUser, "Software\\SophiaWin11Tests\\Y", "Value", RegistryValueKind.DWord);

        var tweak1 = new RegistryTweak(Guid.NewGuid(), "Test", "Tweak1", "desc", target1, 1, 0, false, TweakRiskLevel.Low, virtualizer);
        var tweak2 = new RegistryTweak(Guid.NewGuid(), "Test", "Tweak2", "desc", target2, 1, 0, false, TweakRiskLevel.Medium, virtualizer);

        var restorePointService = new FakeRestorePointService();
        var healthDiagnosticService = new FakeHealthDiagnosticService();
        var service = CreateService(restorePointService, healthDiagnosticService);

        await service.ApplySessionAsync([tweak1, tweak2], "Test session");

        Assert.Equal(1, restorePointService.CallCount);
    }

    [Fact]
    public async Task ApplySessionAsync_LowRiskOnly_DoesNotCreateRestorePoint()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var target1 = new RegistryImpact(RegistryHive.CurrentUser, "Software\\SophiaWin11Tests\\LowOnlyA", "Value", RegistryValueKind.DWord);
        var target2 = new RegistryImpact(RegistryHive.CurrentUser, "Software\\SophiaWin11Tests\\LowOnlyB", "Value", RegistryValueKind.DWord);

        var tweak1 = new RegistryTweak(Guid.NewGuid(), "Test", "Tweak1", "desc", target1, 1, 0, false, TweakRiskLevel.Low, virtualizer);
        var tweak2 = new RegistryTweak(Guid.NewGuid(), "Test", "Tweak2", "desc", target2, 1, 0, false, TweakRiskLevel.Low, virtualizer);

        var restorePointService = new FakeRestorePointService();
        var healthDiagnosticService = new FakeHealthDiagnosticService();
        var service = CreateService(restorePointService, healthDiagnosticService);

        await service.ApplySessionAsync([tweak1, tweak2], "Test session");

        Assert.Equal(0, restorePointService.CallCount);
    }

    [Fact]
    public async Task ApplySessionAsync_WithHighRiskTweak_CreatesRestorePoint()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var target = new RegistryImpact(RegistryHive.LocalMachine, "Software\\SophiaWin11Tests\\HighRiskRestorePoint", "Value", RegistryValueKind.DWord);
        var tweak = new RegistryTweak(Guid.NewGuid(), "Test", "HighRiskTweak", "desc", target, 1, 0, false, TweakRiskLevel.High, virtualizer);

        var restorePointService = new FakeRestorePointService();
        var healthDiagnosticService = new FakeHealthDiagnosticService();
        var service = CreateService(restorePointService, healthDiagnosticService);

        await service.ApplySessionAsync([tweak], "Test session");

        Assert.Equal(1, restorePointService.CallCount);
    }

    [Fact]
    public async Task ApplySessionAsync_WithConflictingTweaks_ThrowsAndAppliesNothing()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var target = new RegistryImpact(RegistryHive.LocalMachine, "Software\\SophiaWin11Tests\\Conflict", "Value", RegistryValueKind.DWord);

        var tweak1 = new RegistryTweak(Guid.NewGuid(), "Test", "Tweak1", "desc", target, 1, 0, false, TweakRiskLevel.Low, virtualizer);
        var tweak2 = new RegistryTweak(Guid.NewGuid(), "Test", "Tweak2", "desc", target, 2, 0, false, TweakRiskLevel.Low, virtualizer);

        var restorePointService = new FakeRestorePointService();
        var healthDiagnosticService = new FakeHealthDiagnosticService();
        var service = CreateService(restorePointService, healthDiagnosticService);

        await Assert.ThrowsAsync<TweakConflictException>(() => service.ApplySessionAsync([tweak1, tweak2], "Test session"));

        Assert.Equal(0, restorePointService.CallCount);
        Assert.Equal(0, virtualizer.WriteCount);
    }

    [Fact]
    public async Task ApplySessionAsync_WithHighRiskTweak_RunsHealthDiagnosticFirst()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var target = new RegistryImpact(RegistryHive.LocalMachine, "Software\\SophiaWin11Tests\\HighRisk", "Value", RegistryValueKind.DWord);
        var tweak = new RegistryTweak(Guid.NewGuid(), "Test", "HighRiskTweak", "desc", target, 1, 0, false, TweakRiskLevel.High, virtualizer);

        var restorePointService = new FakeRestorePointService();
        var healthDiagnosticService = new FakeHealthDiagnosticService();
        var service = CreateService(restorePointService, healthDiagnosticService);

        await service.ApplySessionAsync([tweak], "Test session");

        Assert.Equal(1, healthDiagnosticService.CallCount);
    }

    [Fact]
    public async Task ApplySessionAsync_WithHighRiskTweak_UnhealthySystem_AbortsSession()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var target = new RegistryImpact(RegistryHive.LocalMachine, "Software\\SophiaWin11Tests\\HighRisk2", "Value", RegistryValueKind.DWord);
        var tweak = new RegistryTweak(Guid.NewGuid(), "Test", "HighRiskTweak", "desc", target, 1, 0, false, TweakRiskLevel.High, virtualizer);

        var restorePointService = new FakeRestorePointService();
        var healthDiagnosticService = new FakeHealthDiagnosticService { Healthy = false };
        var service = CreateService(restorePointService, healthDiagnosticService);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApplySessionAsync([tweak], "Test session"));

        Assert.Equal(0, virtualizer.WriteCount);
        Assert.Equal(0, restorePointService.CallCount);
    }

    [Fact]
    public async Task ApplySessionAsync_LowRiskOnly_DoesNotRunHealthDiagnostic()
    {
        var virtualizer = new RegistryHiveVirtualizer();
        var target = new RegistryImpact(RegistryHive.CurrentUser, "Software\\SophiaWin11Tests\\LowRisk", "Value", RegistryValueKind.DWord);
        var tweak = new RegistryTweak(Guid.NewGuid(), "Test", "LowRiskTweak", "desc", target, 1, 0, false, TweakRiskLevel.Low, virtualizer);

        var restorePointService = new FakeRestorePointService();
        var healthDiagnosticService = new FakeHealthDiagnosticService();
        var service = CreateService(restorePointService, healthDiagnosticService);

        await service.ApplySessionAsync([tweak], "Test session");

        Assert.Equal(0, healthDiagnosticService.CallCount);
    }
}
