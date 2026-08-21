namespace SophiaWin11.Core.Abstractions;

public interface IRestorePointService
{
    Task<bool> CreateRestorePointAsync(string description, CancellationToken cancellationToken = default);
}
