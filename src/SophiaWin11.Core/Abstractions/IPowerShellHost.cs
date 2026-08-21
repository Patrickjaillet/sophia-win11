namespace SophiaWin11.Core.Abstractions;

public interface IPowerShellHost
{
    Task InvokeAsync(string script, CancellationToken cancellationToken = default);
}
