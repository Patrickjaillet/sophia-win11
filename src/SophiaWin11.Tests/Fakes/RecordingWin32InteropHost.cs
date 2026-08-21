using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Tests.Fakes;

public sealed class RecordingWin32InteropHost : IWin32InteropHost
{
    public List<(string Operation, IReadOnlyDictionary<string, string> Parameters)> InvokedOperations { get; } = [];

    public Task InvokeAsync(string operation, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        InvokedOperations.Add((operation, parameters));
        return Task.CompletedTask;
    }
}
