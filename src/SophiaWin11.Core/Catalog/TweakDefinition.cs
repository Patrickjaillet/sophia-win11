namespace SophiaWin11.Core.Catalog;

public sealed class TweakCatalogFile
{
    public IReadOnlyList<TweakDefinition> Tweaks { get; set; } = [];
}

public sealed class TweakDefinition
{
    public string Id { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SophiaFunction { get; set; } = string.Empty;

    public string Type { get; set; } = "Registry";

    public string RiskLevel { get; set; } = "Low";

    public bool RequiresRestart { get; set; }

    public string Hive { get; set; } = "CurrentUser";

    public string SubKey { get; set; } = string.Empty;

    public string ValueName { get; set; } = string.Empty;

    public string ValueKind { get; set; } = "DWord";

    public object? ApplyValue { get; set; }

    public object? RevertValue { get; set; }
}
