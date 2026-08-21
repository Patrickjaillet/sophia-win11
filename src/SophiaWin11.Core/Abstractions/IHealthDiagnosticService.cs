namespace SophiaWin11.Core.Abstractions;

public sealed record HealthDiagnosticResult(bool IsHealthy, string Details);

public interface IHealthDiagnosticService
{
    Task<HealthDiagnosticResult> RunAsync(CancellationToken cancellationToken = default);
}
