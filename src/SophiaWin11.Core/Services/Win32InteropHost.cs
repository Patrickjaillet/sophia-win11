using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class Win32InteropHost : IWin32InteropHost
{
    private readonly ILogger<Win32InteropHost> _logger;

    public Win32InteropHost(ILogger<Win32InteropHost> logger)
    {
        _logger = logger;
    }

    public Task InvokeAsync(string operation, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
