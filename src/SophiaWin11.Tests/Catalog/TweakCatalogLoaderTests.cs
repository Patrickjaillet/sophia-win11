using System.IO;
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

    private static TweakCatalogLoader CreateLoader(IRegistryService? registryService = null) =>
        new(
            registryService ?? new RegistryHiveVirtualizer(),
            new RecordingPowerShellHost(),
            new RecordingWin32InteropHost());

    [Fact]
    public void LoadFromJson_ParsesAllEmbeddedTweaks()
    {
        var json = ReadEmbeddedCatalog();
        var loader = CreateLoader();

        var tweaks = loader.LoadFromJson(json);

        Assert.NotEmpty(tweaks);
    }

    [Fact]
    public void LoadFromJson_AllTweaksHaveUniqueIds()
    {
        var json = ReadEmbeddedCatalog();
        var loader = CreateLoader();

        var tweaks = loader.LoadFromJson(json);

        var distinctIds = tweaks.Select(t => t.Id).Distinct().Count();
        Assert.Equal(tweaks.Count, distinctIds);
    }

    [Fact]
    public void LoadFromJson_AllTweaksHaveNonEmptyCategoryAndName()
    {
        var json = ReadEmbeddedCatalog();
        var loader = CreateLoader();

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
        var loader = CreateLoader(virtualizer);

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

    [Fact]
    public async Task LoadFromJson_PowerShellNativeTweak_ApplyAsync_InvokesHostWithApplyScript()
    {
        const string json = """
        {
          "tweaks": [
            {
              "id": "22222222-2222-2222-2222-222222222222",
              "category": "Test",
              "name": "SampleScriptTweak",
              "description": "desc",
              "sophiaFunction": "SampleScriptTweak -Disable",
              "type": "PowerShellNative",
              "riskLevel": "Low",
              "requiresRestart": false,
              "applyScript": "Write-Output 'apply'",
              "revertScript": "Write-Output 'revert'",
              "probeScript": ""
            }
          ]
        }
        """;

        var powerShellHost = new RecordingPowerShellHost();
        var loader = new TweakCatalogLoader(new RegistryHiveVirtualizer(), powerShellHost, new RecordingWin32InteropHost());
        var tweaks = loader.LoadFromJson(json);
        var tweak = Assert.Single(tweaks);

        await tweak.ApplyAsync();

        Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), tweak.Id);
        Assert.Single(powerShellHost.InvokedScripts);
        Assert.Equal("Write-Output 'apply'", powerShellHost.InvokedScripts[0]);

        await tweak.RevertAsync();

        Assert.Equal(2, powerShellHost.InvokedScripts.Count);
        Assert.Equal("Write-Output 'revert'", powerShellHost.InvokedScripts[1]);
    }

    [Fact]
    public void LoadFromJson_PowerShellNativeTweak_WithoutHost_Throws()
    {
        const string json = """
        {
          "tweaks": [
            {
              "id": "22222222-2222-2222-2222-222222222222",
              "category": "Test",
              "name": "SampleScriptTweak",
              "description": "desc",
              "sophiaFunction": "SampleScriptTweak -Disable",
              "type": "PowerShellNative",
              "riskLevel": "Low",
              "requiresRestart": false,
              "applyScript": "Write-Output 'apply'",
              "revertScript": "Write-Output 'revert'",
              "probeScript": ""
            }
          ]
        }
        """;

        var loader = new TweakCatalogLoader(new RegistryHiveVirtualizer());

        Assert.Throws<InvalidOperationException>(() => loader.LoadFromJson(json));
    }

    [Fact]
    public async Task LoadFromJson_Win32ApiTweak_ApplyAsync_InvokesHostWithOperationAndParameters()
    {
        const string json = """
        {
          "tweaks": [
            {
              "id": "33333333-3333-3333-3333-333333333333",
              "category": "Test",
              "name": "SampleWin32Tweak",
              "description": "desc",
              "sophiaFunction": "SampleWin32Tweak -Disable",
              "type": "Win32Api",
              "riskLevel": "Low",
              "requiresRestart": false,
              "operation": "BroadcastSettingChange",
              "applyParameters": { "section": "Cursors" },
              "revertParameters": { "section": "Cursors" }
            }
          ]
        }
        """;

        var win32InteropHost = new RecordingWin32InteropHost();
        var loader = new TweakCatalogLoader(new RegistryHiveVirtualizer(), new RecordingPowerShellHost(), win32InteropHost);
        var tweaks = loader.LoadFromJson(json);
        var tweak = Assert.Single(tweaks);

        await tweak.ApplyAsync();

        Assert.Single(win32InteropHost.InvokedOperations);
        Assert.Equal("BroadcastSettingChange", win32InteropHost.InvokedOperations[0].Operation);
        Assert.Equal("Cursors", win32InteropHost.InvokedOperations[0].Parameters["section"]);
    }
}
