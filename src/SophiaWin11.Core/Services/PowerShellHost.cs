using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class PowerShellHost : IPowerShellHost
{
    private readonly ILogger<PowerShellHost> _logger;

    public PowerShellHost(ILogger<PowerShellHost> logger)
    {
        _logger = logger;
    }

    public Task InvokeAsync(string script, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
