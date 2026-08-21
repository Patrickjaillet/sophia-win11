using System.Reflection;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Catalog;
using SophiaWin11.Tests.Fakes;
using Xunit;

namespace SophiaWin11.Tests.Catalog;

public sealed class TweakCatalogLoaderTests
{
    private static string ReadEmbeddedCatalog()
    {
        var assembly = Assembly.Load("SophiaWin11.Core");
        using var stream = assembly.GetManifestResourceStream("SophiaWin11.Core.Catalog.tweaks.en.json")
                            ?? throw new InvalidOperationException("Embedded catalog not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [Fact]
    public void LoadFromJson_ParsesAllEmbeddedTweaks()
    {
        var json = ReadEmbeddedCatalog();
        var loader = new TweakCatalogLoader(new RegistryHiveVirtualizer());

        var tweaks = loader.LoadFromJson(json);

        Assert.NotEmpty(tweaks);
    }

    [Fact]
    public void LoadFromJson_AllTweaksHaveUniqueIds()
    {
        var json = ReadEmbeddedCatalog();
        var loader = new TweakCatalogLoader(new RegistryHiveVirtualizer());

        var tweaks = loader.LoadFromJson(json);

        var distinctIds = tweaks.Select(t => t.Id).Distinct().Count();
        Assert.Equal(tweaks.Count, distinctIds);
    }

    [Fact]
    public void LoadFromJson_AllTweaksHaveNonEmptyCategoryAndName()
    {
        var json = ReadEmbeddedCatalog();
        var loader = new TweakCatalogLoader(new RegistryHiveVirtualizer());

        var tweaks = loader.LoadFromJson(json);

        Assert.All(tweaks, tweak =>
        {
            Assert.False(string.IsNullOrWhiteSpace(tweak.Category));
            Assert.False(string.IsNullOrWhiteSpace(tweak.Name));
        });
    }

    [Fact]
    public async Task LoadFromJson_ProducedTweaks_CanApplyAndRevertAgainstVirtualizer()
    {
        var json = ReadEmbeddedCatalog();
        var virtualizer = new RegistryHiveVirtualizer();
        var loader = new TweakCatalogLoader(virtualizer);

        var tweaks = loader.LoadFromJson(json);
        var first = tweaks[0];

        await first.ApplyAsync();
        Assert.True(await first.IsAppliedAsync());

        await first.RevertAsync();
        Assert.False(await first.IsAppliedAsync());
    }

    [Fact]
    public void LoadFromJson_MinimalSingleTweak_ProducesCorrectRegistryImpact()
    {
        const string json = """
        {
          "tweaks": [
            {
              "id": "11111111-1111-1111-1111-111111111111",
              "category": "Test",
              "name": "SampleTweak",
              "description": "desc",
              "sophiaFunction": "SampleTweak -Disable",
              "type": "Registry",
              "riskLevel": "Low",
              "requiresRestart": false,
              "hive": "CurrentUser",
              "subKey": "Software\\SampleKey",
              "valueName": "SampleValue",
              "valueKind": "DWord",
              "applyValue": 1,
              "revertValue": 0
            }
          ]
        }
        """;

        var loader = new TweakCatalogLoader(new RegistryHiveVirtualizer());
        var tweaks = loader.LoadFromJson(json);

        var tweak = Assert.Single(tweaks);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), tweak.Id);
        Assert.Equal("SampleTweak", tweak.Name);
        Assert.Single(tweak.RegistryImpact);
        Assert.Equal(RegistryHive.CurrentUser, tweak.RegistryImpact[0].Hive);
        Assert.Equal("Software\\SampleKey", tweak.RegistryImpact[0].SubKey);
    }
}
