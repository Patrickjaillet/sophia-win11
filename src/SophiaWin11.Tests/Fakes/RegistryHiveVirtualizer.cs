using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Tests.Fakes;

public sealed class RegistryHiveVirtualizer : IRegistryService
{
    private readonly Dictionary<(RegistryHive Hive, string SubKey, string ValueName), object> _store = new();

    public int WriteCount { get; private set; }

    public int DeleteCount { get; private set; }

    public bool KeyExists(RegistryHive hive, string subKey) =>
        _store.Keys.Any(key => key.Hive == hive && key.SubKey.Equals(subKey, StringComparison.OrdinalIgnoreCase));

    public object? GetValue(RegistryHive hive, string subKey, string valueName) =>
        _store.TryGetValue((hive, subKey, valueName), out var value) ? value : null;

    public void SetValue(RegistryHive hive, string subKey, string valueName, object value, RegistryValueKind kind)
    {
        _store[(hive, subKey, valueName)] = value;
        WriteCount++;
    }

    public void DeleteValue(RegistryHive hive, string subKey, string valueName)
    {
        _store.Remove((hive, subKey, valueName));
        DeleteCount++;
    }

    public void Seed(RegistryHive hive, string subKey, string valueName, object value) =>
        _store[(hive, subKey, valueName)] = value;
}
