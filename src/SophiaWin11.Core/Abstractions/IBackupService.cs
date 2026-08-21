namespace SophiaWin11.Core.Abstractions;

public interface IBackupService
{
    Task<string> CreateSnapshotAsync(CancellationToken cancellationToken = default);

    Task RestoreSnapshotAsync(string snapshotPath, CancellationToken cancellationToken = default);
}
