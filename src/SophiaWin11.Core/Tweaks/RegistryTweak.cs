using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Tweaks;

public sealed class RegistryTweak : TweakBase
{
    private readonly IRegistryService _registryService;
    private readonly RegistryImpact _target;
    private readonly object _applyValue;
    private readonly object? _revertValue;

    public RegistryTweak(
        Guid id,
        string category,
        string name,
        string description,
        RegistryImpact target,
        object applyValue,
        object? revertValue,
        bool requiresRestart,
        TweakRiskLevel riskLevel,
        IRegistryService registryService,
        ITweakSnapshotService? snapshotService = null)
        : base(id, category, name, description, [target], requiresRestart, riskLevel, snapshotService)
    {
        _registryService = registryService;
        _target = target;
        _applyValue = applyValue;
        _revertValue = revertValue;
    }

    protected override Task ApplyCoreAsync(CancellationToken cancellationToken)
    {
        _registryService.SetValue(_target.Hive, _target.SubKey, _target.ValueName, _applyValue, _target.ValueKind);
        return Task.CompletedTask;
    }

    protected override Task RevertCoreAsync(CancellationToken cancellationToken)
    {
        if (_revertValue is null)
        {
            _registryService.DeleteValue(_target.Hive, _target.SubKey, _target.ValueName);
        }
        else
        {
            _registryService.SetValue(_target.Hive, _target.SubKey, _target.ValueName, _revertValue, _target.ValueKind);
        }

        return Task.CompletedTask;
    }

    protected override Task<bool> IsAppliedCoreAsync(CancellationToken cancellationToken)
    {
        var current = _registryService.GetValue(_target.Hive, _target.SubKey, _target.ValueName);
        return Task.FromResult(Equals(current, _applyValue));
    }
}
