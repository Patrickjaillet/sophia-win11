using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Tests.Fakes;

public sealed class RecordingPowerShellHost : IPowerShellHost
{
    public List<string> InvokedScripts { get; } = [];

    public Dictionary<string, IReadOnlyList<string>> CannedOutput { get; } = [];

    public IReadOnlyList<string> DefaultOutput { get; set; } = [];

    public Task InvokeAsync(string script, CancellationToken cancellationToken = default)
    {
        InvokedScripts.Add(script);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> InvokeAndCaptureAsync(string script, CancellationToken cancellationToken = default)
    {
        InvokedScripts.Add(script);

        foreach (var (key, output) in CannedOutput)
        {
            if (script.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(output);
            }
        }

        return Task.FromResult(DefaultOutput);
    }
}
