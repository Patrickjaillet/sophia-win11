using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Tests.Fakes;

public sealed class RecordingPowerShellHost : IPowerShellHost
{
    public List<string> InvokedScripts { get; } = [];

    public Task InvokeAsync(string script, CancellationToken cancellationToken = default)
    {
        InvokedScripts.Add(script);
        return Task.CompletedTask;
    }
}
