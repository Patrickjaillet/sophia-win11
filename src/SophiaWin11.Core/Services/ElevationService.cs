using System.Security.Principal;
using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class ElevationService : IElevationService
{
    private readonly ILogger<ElevationService> _logger;

    public ElevationService(ILogger<ElevationService> logger)
    {
        _logger = logger;
    }

    public bool IsRunningElevated
    {
        get
        {
            if (!OperatingSystem.IsWindows())
            {
                return false;
            }

            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    public void EnsureElevated()
    {
        if (!IsRunningElevated)
        {
            throw new InvalidOperationException("SophiaWin11 requires administrator privileges.");
        }
    }
}
