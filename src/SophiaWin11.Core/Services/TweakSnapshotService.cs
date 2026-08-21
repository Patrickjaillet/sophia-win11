using System.Text.Json;
using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class TweakSnapshotService : ITweakSnapshotService
{
    private readonly IRegistryService _registryService;
    private readonly ILogger<TweakSnapshotService> _logger;
    private readonly string _snapshotDirectory;

    public TweakSnapshotService(IRegistryService registryService, ILogger<TweakSnapshotService> logger)
    {
        _registryService = registryService;
        _logger = logger;
        _snapshotDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SophiaWin11",
            "snapshots");
    }

    public Task<string> CaptureAsync(ITweak tweak, CancellationToken cancellationToken = default)
    {
        var snapshots = new List<RegistryValueSnapshot>(tweak.RegistryImpact.Count);

        foreach (var impact in tweak.RegistryImpact)
        {
            var exists = _registryService.KeyExists(impact.Hive, impact.SubKey);
            var value = exists ? _registryService.GetValue(impact.Hive, impact.SubKey, impact.ValueName) : null;
            snapshots.Add(new RegistryValueSnapshot(impact.Hive, impact.SubKey, impact.ValueName, value, exists && value is not null));
        }

        Directory.CreateDirectory(_snapshotDirectory);
        var fileName = $"{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}_{tweak.Id}.json";
        var path = Path.Combine(_snapshotDirectory, fileName);
        var json = JsonSerializer.Serialize(snapshots);
        File.WriteAllText(path, json);

        _logger.LogInformation("Snapshot captured for tweak {TweakId} at {Path}.", tweak.Id, path);

        return Task.FromResult(path);
    }

    public Task<IReadOnlyList<RegistryValueSnapshot>> ReadAsync(string snapshotPath, CancellationToken cancellationToken = default)
    {
        var json = File.ReadAllText(snapshotPath);
        var snapshots = JsonSerializer.Deserialize<List<RegistryValueSnapshot>>(json) ?? [];
        return Task.FromResult<IReadOnlyList<RegistryValueSnapshot>>(snapshots);
    }
}
