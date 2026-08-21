using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class HealthDiagnosticService : IHealthDiagnosticService
{
    private const string DismCheckHealthScript = "dism.exe /Online /Cleanup-Image /CheckHealth";
    private const string SfcVerifyScript = "sfc /verifyonly";
    private const string DismHealthyMarker = "No component store corruption detected";
    private const string SfcHealthyMarker = "did not find any integrity violations";

    private readonly IPowerShellHost _powerShellHost;
    private readonly ILogger<HealthDiagnosticService> _logger;

    public HealthDiagnosticService(IPowerShellHost powerShellHost, ILogger<HealthDiagnosticService> logger)
    {
        _powerShellHost = powerShellHost;
        _logger = logger;
    }

    public async Task<HealthDiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var dismOutput = await _powerShellHost.InvokeAndCaptureAsync(DismCheckHealthScript, cancellationToken).ConfigureAwait(false);
        var sfcOutput = await _powerShellHost.InvokeAndCaptureAsync(SfcVerifyScript, cancellationToken).ConfigureAwait(false);

        var dismText = string.Join(Environment.NewLine, dismOutput);
        var sfcText = string.Join(Environment.NewLine, sfcOutput);

        var dismHealthy = dismText.Contains(DismHealthyMarker, StringComparison.OrdinalIgnoreCase);
        var sfcHealthy = sfcText.Contains(SfcHealthyMarker, StringComparison.OrdinalIgnoreCase);
        var isHealthy = dismHealthy && sfcHealthy;

        var details = $"DISM image health: {(dismHealthy ? "healthy" : "issues detected")}. " +
                       $"SFC system files: {(sfcHealthy ? "healthy" : "issues detected")}.";

        _logger.LogInformation("System health diagnostic completed. Healthy: {IsHealthy}. {Details}", isHealthy, details);

        return new HealthDiagnosticResult(isHealthy, details);
    }
}
