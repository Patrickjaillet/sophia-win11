using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

[SupportedOSPlatform("windows")]
public sealed class PowerShellHost : IPowerShellHost
{
    private readonly ILogger<PowerShellHost> _logger;

    public PowerShellHost(ILogger<PowerShellHost> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(string script, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(script))
        {
            return;
        }

        var initialSessionState = InitialSessionState.CreateDefault();
        initialSessionState.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Bypass;

        using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();

        using var powerShell = System.Management.Automation.PowerShell.Create();
        powerShell.Runspace = runspace;
        powerShell.AddScript(script);

        using var registration = cancellationToken.Register(() =>
        {
            try
            {
                powerShell.Stop();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to stop the in-process PowerShell pipeline.");
            }
        });

        var results = await Task.Factory.FromAsync(
            powerShell.BeginInvoke(),
            powerShell.EndInvoke).ConfigureAwait(false);

        foreach (var record in powerShell.Streams.Error)
        {
            _logger.LogError("PowerShell error: {Error}", record.ToString());
        }

        if (powerShell.HadErrors)
        {
            var firstError = powerShell.Streams.Error.Count > 0
                ? powerShell.Streams.Error[0].ToString()
                : "Unknown PowerShell error.";
            throw new InvalidOperationException($"PowerShell script failed: {firstError}");
        }

        _logger.LogInformation("PowerShell script executed with {Count} output records.", results.Count);
    }
}
