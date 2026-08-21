using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Tweaks;

public abstract class TweakBase : ITweak
{
    private readonly ITweakSnapshotService? _snapshotService;

    protected TweakBase(
        Guid id,
        string category,
        string name,
        string description,
        IReadOnlyList<RegistryImpact> registryImpact,
        bool requiresRestart,
        TweakRiskLevel riskLevel,
        ITweakSnapshotService? snapshotService = null)
    {
        Id = id;
        Category = category;
        Name = name;
        Description = description;
        RegistryImpact = registryImpact;
        RequiresRestart = requiresRestart;
        RiskLevel = riskLevel;
        _snapshotService = snapshotService;
    }

    public Guid Id { get; }

    public string Category { get; }

    public string Name { get; }

    public string Description { get; }

    public IReadOnlyList<RegistryImpact> RegistryImpact { get; }

    public bool RequiresRestart { get; }

    public TweakRiskLevel RiskLevel { get; }

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        if (RiskLevel is TweakRiskLevel.Medium or TweakRiskLevel.High && _snapshotService is not null)
        {
            await _snapshotService.CaptureAsync(this, cancellationToken).ConfigureAwait(false);
        }

        await ApplyCoreAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task RevertAsync(CancellationToken cancellationToken = default) => RevertCoreAsync(cancellationToken);

    public Task<bool> IsAppliedAsync(CancellationToken cancellationToken = default) => IsAppliedCoreAsync(cancellationToken);

    public Task<string> PreviewAsync(CancellationToken cancellationToken = default) => PreviewCoreAsync(cancellationToken);

    protected abstract Task ApplyCoreAsync(CancellationToken cancellationToken);

    protected abstract Task RevertCoreAsync(CancellationToken cancellationToken);

    protected abstract Task<bool> IsAppliedCoreAsync(CancellationToken cancellationToken);

    protected abstract Task<string> PreviewCoreAsync(CancellationToken cancellationToken);
}
