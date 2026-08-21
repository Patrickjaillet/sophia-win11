using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Tweaks;

public sealed class PowerShellNativeTweak : TweakBase
{
    private readonly IPowerShellHost _powerShellHost;
    private readonly string _applyScript;
    private readonly string _revertScript;
    private readonly string _probeScript;

    public PowerShellNativeTweak(
        Guid id,
        string category,
        string name,
        string description,
        string applyScript,
        string revertScript,
        string probeScript,
        bool requiresRestart,
        TweakRiskLevel riskLevel,
        IPowerShellHost powerShellHost,
        IReadOnlyList<RegistryImpact>? registryImpact = null,
        ITweakSnapshotService? snapshotService = null)
        : base(id, category, name, description, registryImpact ?? [], requiresRestart, riskLevel, snapshotService)
    {
        _powerShellHost = powerShellHost;
        _applyScript = applyScript;
        _revertScript = revertScript;
        _probeScript = probeScript;
    }

    protected override Task ApplyCoreAsync(CancellationToken cancellationToken) =>
        _powerShellHost.InvokeAsync(_applyScript, cancellationToken);

    protected override Task RevertCoreAsync(CancellationToken cancellationToken) =>
        _powerShellHost.InvokeAsync(_revertScript, cancellationToken);

    protected override async Task<bool> IsAppliedCoreAsync(CancellationToken cancellationToken)
    {
        await _powerShellHost.InvokeAsync(_probeScript, cancellationToken).ConfigureAwait(false);
        return false;
    }

    protected override Task<string> PreviewCoreAsync(CancellationToken cancellationToken)
    {
        var preview = string.IsNullOrWhiteSpace(_applyScript)
            ? "No apply script configured."
            : $"The following PowerShell script would run:{Environment.NewLine}{_applyScript}";
        return Task.FromResult(preview);
    }
}
