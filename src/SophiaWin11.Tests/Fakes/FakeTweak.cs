using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Tests.Fakes;

public sealed class FakeTweak : ITweak
{
    private bool _isApplied;

    public FakeTweak(
        string category,
        string name,
        string description,
        TweakRiskLevel riskLevel = TweakRiskLevel.Low,
        bool requiresRestart = false,
        bool initiallyApplied = false)
    {
        Id = Guid.NewGuid();
        Category = category;
        Name = name;
        Description = description;
        RiskLevel = riskLevel;
        RequiresRestart = requiresRestart;
        _isApplied = initiallyApplied;
    }

    public Guid Id { get; }

    public string Category { get; }

    public string Name { get; }

    public string Description { get; }

    public IReadOnlyList<RegistryImpact> RegistryImpact => [];

    public bool RequiresRestart { get; }

    public TweakRiskLevel RiskLevel { get; }

    public int ApplyCallCount { get; private set; }

    public int RevertCallCount { get; private set; }

    public bool ThrowOnApply { get; set; }

    public Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        ApplyCallCount++;

        if (ThrowOnApply)
        {
            throw new InvalidOperationException("Simulated apply failure.");
        }

        _isApplied = true;
        return Task.CompletedTask;
    }

    public Task RevertAsync(CancellationToken cancellationToken = default)
    {
        RevertCallCount++;
        _isApplied = false;
        return Task.CompletedTask;
    }

    public Task<bool> IsAppliedAsync(CancellationToken cancellationToken = default) => Task.FromResult(_isApplied);

    public Task<string> PreviewAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
}
