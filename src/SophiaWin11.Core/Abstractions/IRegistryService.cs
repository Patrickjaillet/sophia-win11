namespace SophiaWin11.Core.Abstractions;

public enum RegistryHive
{
    ClassesRoot,
    CurrentUser,
    LocalMachine,
    Users,
    CurrentConfig,
}

public enum RegistryValueKind
{
    String,
    ExpandString,
    Binary,
    DWord,
    MultiString,
    QWord,
}

public interface IRegistryService
{
    bool KeyExists(RegistryHive hive, string subKey);

    object? GetValue(RegistryHive hive, string subKey, string valueName);

    void SetValue(RegistryHive hive, string subKey, string valueName, object value, RegistryValueKind kind);

    void DeleteValue(RegistryHive hive, string subKey, string valueName);
}
