using System.Text.Json;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Tweaks;

namespace SophiaWin11.Core.Catalog;

public sealed class TweakCatalogLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IRegistryService _registryService;
    private readonly ITweakSnapshotService? _snapshotService;

    public TweakCatalogLoader(IRegistryService registryService, ITweakSnapshotService? snapshotService = null)
    {
        _registryService = registryService;
        _snapshotService = snapshotService;
    }

    public IReadOnlyList<ITweak> LoadFromJson(string json)
    {
        var file = JsonSerializer.Deserialize<TweakCatalogFile>(json, SerializerOptions)
                   ?? throw new InvalidOperationException("Tweak catalog JSON could not be parsed.");

        var tweaks = new List<ITweak>(file.Tweaks.Count);

        foreach (var definition in file.Tweaks)
        {
            tweaks.Add(CreateTweak(definition));
        }

        return tweaks;
    }

    private ITweak CreateTweak(TweakDefinition definition)
    {
        if (!Enum.TryParse<TweakRiskLevel>(definition.RiskLevel, ignoreCase: true, out var riskLevel))
        {
            throw new InvalidOperationException($"Unknown risk level '{definition.RiskLevel}' for tweak '{definition.Name}'.");
        }

        if (!Enum.TryParse<RegistryHive>(definition.Hive, ignoreCase: true, out var hive))
        {
            throw new InvalidOperationException($"Unknown registry hive '{definition.Hive}' for tweak '{definition.Name}'.");
        }

        if (!Enum.TryParse<RegistryValueKind>(definition.ValueKind, ignoreCase: true, out var valueKind))
        {
            throw new InvalidOperationException($"Unknown value kind '{definition.ValueKind}' for tweak '{definition.Name}'.");
        }

        var impact = new RegistryImpact(hive, definition.SubKey, definition.ValueName, valueKind);

        var applyValue = ConvertValue(definition.ApplyValue, valueKind)
                          ?? throw new InvalidOperationException($"Tweak '{definition.Name}' is missing an apply value.");
        var revertValue = ConvertValue(definition.RevertValue, valueKind);

        return new RegistryTweak(
            Guid.Parse(definition.Id),
            definition.Category,
            definition.Name,
            definition.Description,
            impact,
            applyValue,
            revertValue,
            definition.RequiresRestart,
            riskLevel,
            _registryService,
            _snapshotService);
    }

    private static object? ConvertValue(object? rawValue, RegistryValueKind kind)
    {
        if (rawValue is null)
        {
            return null;
        }

        if (rawValue is not JsonElement element)
        {
            return rawValue;
        }

        return kind switch
        {
            RegistryValueKind.DWord => element.GetInt32(),
            RegistryValueKind.QWord => element.GetInt64(),
            RegistryValueKind.String or RegistryValueKind.ExpandString => element.GetString(),
            RegistryValueKind.MultiString => element.EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToArray(),
            _ => element.GetRawText(),
        };
    }
}
