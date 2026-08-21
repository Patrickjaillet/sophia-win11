using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class RegistryService : IRegistryService
{
    private readonly ILogger<RegistryService> _logger;

    public RegistryService(ILogger<RegistryService> logger)
    {
        _logger = logger;
    }

    public bool KeyExists(RegistryHive hive, string subKey)
    {
        throw new NotImplementedException();
    }

    public object? GetValue(RegistryHive hive, string subKey, string valueName)
    {
        throw new NotImplementedException();
    }

    public void SetValue(RegistryHive hive, string subKey, string valueName, object value, RegistryValueKind kind)
    {
        throw new NotImplementedException();
    }

    public void DeleteValue(RegistryHive hive, string subKey, string valueName)
    {
        throw new NotImplementedException();
    }
}
