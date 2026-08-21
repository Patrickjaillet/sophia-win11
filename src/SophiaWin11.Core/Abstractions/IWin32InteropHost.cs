namespace SophiaWin11.Core.Abstractions;

public interface IWin32InteropHost
{
    Task InvokeAsync(string operation, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default);
}
