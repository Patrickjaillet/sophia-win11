using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Tweaks;

public sealed class Win32ApiTweak : TweakBase
{
    private readonly IWin32InteropHost _win32InteropHost;
    private readonly string _operation;
    private readonly IReadOnlyDictionary<string, string> _applyParameters;
    private readonly IReadOnlyDictionary<string, string> _revertParameters;

    public Win32ApiTweak(
        Guid id,
        string category,
        string name,
        string description,
        string operation,
        IReadOnlyDictionary<string, string> applyParameters,
        IReadOnlyDictionary<string, string> revertParameters,
        bool requiresRestart,
        TweakRiskLevel riskLevel,
        IWin32InteropHost win32InteropHost,
        ITweakSnapshotService? snapshotService = null)
        : base(id, category, name, description, [], requiresRestart, riskLevel, snapshotService)
    {
        _win32InteropHost = win32InteropHost;
        _operation = operation;
        _applyParameters = applyParameters;
        _revertParameters = revertParameters;
    }

    protected override Task ApplyCoreAsync(CancellationToken cancellationToken) =>
        _win32InteropHost.InvokeAsync(_operation, _applyParameters, cancellationToken);

    protected override Task RevertCoreAsync(CancellationToken cancellationToken) =>
        _win32InteropHost.InvokeAsync(_operation, _revertParameters, cancellationToken);

    protected override Task<bool> IsAppliedCoreAsync(CancellationToken cancellationToken) =>
        Task.FromResult(false);

    protected override Task<string> PreviewCoreAsync(CancellationToken cancellationToken)
    {
        var parameters = _applyParameters.Count == 0
            ? "(no parameters)"
            : string.Join(", ", _applyParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        var preview = $"Would invoke Win32 operation '{_operation}' with parameters: {parameters}";
        return Task.FromResult(preview);
    }
}
