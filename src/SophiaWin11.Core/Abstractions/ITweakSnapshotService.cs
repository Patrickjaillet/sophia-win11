namespace SophiaWin11.Core.Abstractions;

public sealed record RegistryValueSnapshot(
    RegistryHive Hive,
    string SubKey,
    string ValueName,
    object? PreviousValue,
    bool ValueExisted);

public interface ITweakSnapshotService
{
    Task<string> CaptureAsync(ITweak tweak, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RegistryValueSnapshot>> ReadAsync(string snapshotPath, CancellationToken cancellationToken = default);
}
