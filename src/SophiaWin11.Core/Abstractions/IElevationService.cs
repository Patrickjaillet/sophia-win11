namespace SophiaWin11.Core.Abstractions;

public interface IElevationService
{
    bool IsRunningElevated { get; }

    void EnsureElevated();
}
