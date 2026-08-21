using Microsoft.Extensions.Logging.Abstractions;
using SophiaWin11.Core.Services;
using SophiaWin11.Tests.Fakes;
using Xunit;

namespace SophiaWin11.Tests.Services;

public sealed class HealthDiagnosticServiceTests
{
    [Fact]
    public async Task RunAsync_DismAndSfcClean_ReturnsHealthy()
    {
        var powerShellHost = new RecordingPowerShellHost();
        powerShellHost.CannedOutput["dism.exe"] = ["Deployment Image Servicing and Management tool", "No component store corruption detected."];
        powerShellHost.CannedOutput["sfc"] = ["Windows Resource Protection did not find any integrity violations."];

        var service = new HealthDiagnosticService(powerShellHost, NullLogger<HealthDiagnosticService>.Instance);
        var result = await service.RunAsync();

        Assert.True(result.IsHealthy);
    }

    [Fact]
    public async Task RunAsync_DismReportsCorruption_ReturnsUnhealthy()
    {
        var powerShellHost = new RecordingPowerShellHost();
        powerShellHost.CannedOutput["dism.exe"] = ["The component store is repairable."];
        powerShellHost.CannedOutput["sfc"] = ["Windows Resource Protection did not find any integrity violations."];

        var service = new HealthDiagnosticService(powerShellHost, NullLogger<HealthDiagnosticService>.Instance);
        var result = await service.RunAsync();

        Assert.False(result.IsHealthy);
    }

    [Fact]
    public async Task RunAsync_SfcReportsViolations_ReturnsUnhealthy()
    {
        var powerShellHost = new RecordingPowerShellHost();
        powerShellHost.CannedOutput["dism.exe"] = ["No component store corruption detected."];
        powerShellHost.CannedOutput["sfc"] = ["Windows Resource Protection found corrupt files but was unable to fix some of them."];

        var service = new HealthDiagnosticService(powerShellHost, NullLogger<HealthDiagnosticService>.Instance);
        var result = await service.RunAsync();

        Assert.False(result.IsHealthy);
    }

    [Fact]
    public async Task RunAsync_InvokesBothDismAndSfcScripts()
    {
        var powerShellHost = new RecordingPowerShellHost();

        var service = new HealthDiagnosticService(powerShellHost, NullLogger<HealthDiagnosticService>.Instance);
        await service.RunAsync();

        Assert.Equal(2, powerShellHost.InvokedScripts.Count);
        Assert.Contains(powerShellHost.InvokedScripts, s => s.Contains("dism.exe", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(powerShellHost.InvokedScripts, s => s.Contains("sfc", StringComparison.OrdinalIgnoreCase));
    }
}
