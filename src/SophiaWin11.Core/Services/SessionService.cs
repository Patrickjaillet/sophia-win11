using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class SessionService : ISessionService
{
    private readonly IConflictDetectionService _conflictDetectionService;
    private readonly IRestorePointService _restorePointService;
    private readonly IHealthDiagnosticService _healthDiagnosticService;
    private readonly ILogger<SessionService> _logger;

    public SessionService(
        IConflictDetectionService conflictDetectionService,
        IRestorePointService restorePointService,
        IHealthDiagnosticService healthDiagnosticService,
        ILogger<SessionService> logger)
    {
        _conflictDetectionService = conflictDetectionService;
        _restorePointService = restorePointService;
        _healthDiagnosticService = healthDiagnosticService;
        _logger = logger;
    }

    public async Task<TweakSession> ApplySessionAsync(
        IEnumerable<ITweak> tweaks,
        string description,
        CancellationToken cancellationToken = default)
    {
        var selection = tweaks.ToList();

        var conflicts = _conflictDetectionService.DetectConflicts(selection);
        if (conflicts.Count > 0)
        {
            _logger.LogWarning("Aborting session apply: {Count} conflict(s) detected.", conflicts.Count);
            throw new TweakConflictException(conflicts);
        }

        HealthDiagnosticResult? healthDiagnostic = null;
        if (selection.Any(tweak => tweak.RiskLevel == TweakRiskLevel.High))
        {
            healthDiagnostic = await _healthDiagnosticService.RunAsync(cancellationToken).ConfigureAwait(false);
            if (!healthDiagnostic.IsHealthy)
            {
                _logger.LogWarning("Aborting session apply: system health check failed. {Details}", healthDiagnostic.Details);
                throw new InvalidOperationException(
                    $"System health check failed, aborting a session that contains High-risk tweaks: {healthDiagnostic.Details}");
            }
        }

        var restorePointCreated = false;
        if (selection.Any(tweak => tweak.RiskLevel is TweakRiskLevel.Medium or TweakRiskLevel.High))
        {
            restorePointCreated = await _restorePointService
                .CreateRestorePointAsync(description, cancellationToken)
                .ConfigureAwait(false);

            if (!restorePointCreated)
            {
                _logger.LogWarning("System Restore point could not be created; proceeding with the session anyway.");
            }
        }

        var applied = new List<ITweak>(selection.Count);
        try
        {
            foreach (var tweak in selection)
            {
                await tweak.ApplyAsync(cancellationToken).ConfigureAwait(false);
                applied.Add(tweak);
            }
        }
        catch
        {
            _logger.LogError("A tweak failed during session apply; rolling back {Count} already-applied tweak(s).", applied.Count);
            await RollbackTweaksAsync(applied, cancellationToken).ConfigureAwait(false);
            throw;
        }

        var session = new TweakSession(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            applied,
            restorePointCreated,
            healthDiagnostic);

        _logger.LogInformation("Session {SessionId} applied {Count} tweak(s).", session.Id, applied.Count);

        return session;
    }

    public Task RollbackSessionAsync(TweakSession session, CancellationToken cancellationToken = default) =>
        RollbackTweaksAsync(session.AppliedTweaks, cancellationToken);

    private async Task RollbackTweaksAsync(IReadOnlyList<ITweak> appliedTweaks, CancellationToken cancellationToken)
    {
        for (var i = appliedTweaks.Count - 1; i >= 0; i--)
        {
            await appliedTweaks[i].RevertAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
