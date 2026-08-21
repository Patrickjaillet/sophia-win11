using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public sealed class RegistryService : IRegistryService
{
    private readonly ILogger<RegistryService> _logger;

    public RegistryService(ILogger<RegistryService> logger)
    {
        _logger = logger;
    }

    public bool KeyExists(RegistryHive hive, string subKey)
    {
        EnsureWindows();
        using var baseKey = OpenBaseKey(hive);
        using var key = baseKey.OpenSubKey(subKey);
        return key is not null;
    }

    public object? GetValue(RegistryHive hive, string subKey, string valueName)
    {
        EnsureWindows();
        using var baseKey = OpenBaseKey(hive);
        using var key = baseKey.OpenSubKey(subKey);
        return key?.GetValue(valueName);
    }

    public void SetValue(RegistryHive hive, string subKey, string valueName, object value, RegistryValueKind kind)
    {
        EnsureWindows();
        using var baseKey = OpenBaseKey(hive);
        using var key = baseKey.CreateSubKey(subKey, writable: true)
                        ?? throw new InvalidOperationException($"Unable to create or open registry key '{subKey}'.");
        key.SetValue(valueName, value, ToMicrosoftKind(kind));
        _logger.LogInformation("Registry value {SubKey}\\{ValueName} set.", subKey, valueName);
    }

    public void DeleteValue(RegistryHive hive, string subKey, string valueName)
    {
        EnsureWindows();
        using var baseKey = OpenBaseKey(hive);
        using var key = baseKey.OpenSubKey(subKey, writable: true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }

    private static void EnsureWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Registry access is only available on Windows.");
        }
    }

    private static Microsoft.Win32.RegistryKey OpenBaseKey(RegistryHive hive) => hive switch
    {
        RegistryHive.ClassesRoot => Microsoft.Win32.Registry.ClassesRoot,
        RegistryHive.CurrentUser => Microsoft.Win32.Registry.CurrentUser,
        RegistryHive.LocalMachine => Microsoft.Win32.Registry.LocalMachine,
        RegistryHive.Users => Microsoft.Win32.Registry.Users,
        RegistryHive.CurrentConfig => Microsoft.Win32.Registry.CurrentConfig,
        _ => throw new ArgumentOutOfRangeException(nameof(hive), hive, null),
    };

    private static Microsoft.Win32.RegistryValueKind ToMicrosoftKind(RegistryValueKind kind) => kind switch
    {
        RegistryValueKind.String => Microsoft.Win32.RegistryValueKind.String,
        RegistryValueKind.ExpandString => Microsoft.Win32.RegistryValueKind.ExpandString,
        RegistryValueKind.Binary => Microsoft.Win32.RegistryValueKind.Binary,
        RegistryValueKind.DWord => Microsoft.Win32.RegistryValueKind.DWord,
        RegistryValueKind.MultiString => Microsoft.Win32.RegistryValueKind.MultiString,
        RegistryValueKind.QWord => Microsoft.Win32.RegistryValueKind.QWord,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };
}
