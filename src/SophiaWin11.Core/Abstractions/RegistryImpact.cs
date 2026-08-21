namespace SophiaWin11.Core.Abstractions;

public sealed record RegistryImpact(
    RegistryHive Hive,
    string SubKey,
    string ValueName,
    RegistryValueKind ValueKind,
    object? ApplyValue = null);
