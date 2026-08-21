using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class BackupService : IBackupService
{
    private readonly ILogger<BackupService> _logger;

    public BackupService(ILogger<BackupService> logger)
    {
        _logger = logger;
    }

    public Task<string> CreateSnapshotAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RestoreSnapshotAsync(string snapshotPath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
