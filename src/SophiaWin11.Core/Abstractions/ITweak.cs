namespace SophiaWin11.Core.Abstractions;

public interface ITweak
{
    Guid Id { get; }

    string Category { get; }

    string Name { get; }

    string Description { get; }

    IReadOnlyList<RegistryImpact> RegistryImpact { get; }

    bool RequiresRestart { get; }

    TweakRiskLevel RiskLevel { get; }

    Task ApplyAsync(CancellationToken cancellationToken = default);

    Task RevertAsync(CancellationToken cancellationToken = default);

    Task<bool> IsAppliedAsync(CancellationToken cancellationToken = default);

    Task<string> PreviewAsync(CancellationToken cancellationToken = default);
}
