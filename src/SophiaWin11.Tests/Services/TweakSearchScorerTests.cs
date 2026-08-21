using SophiaWin11.App.Services;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Tests.Fakes;
using Xunit;

namespace SophiaWin11.Tests.Services;

public sealed class TweakSearchScorerTests
{
    private static readonly IReadOnlyList<ITweak> Tweaks =
    [
        new FakeTweak("Privacy & Telemetry", "DiagTrackService", "Disable the Connected User Experiences and Telemetry service."),
        new FakeTweak("UI & Personalization", "DarkTheme", "Enable dark theme across the shell."),
        new FakeTweak("Gaming", "GameBar", "Disable the Xbox Game Bar overlay."),
    ];

    [Fact]
    public void Search_EmptyQuery_ReturnsAllTweaks()
    {
        var results = TweakSearchScorer.Search(string.Empty, Tweaks);

        Assert.Equal(Tweaks.Count, results.Count);
    }

    [Fact]
    public void Search_ExactNameMatch_ReturnsThatTweakFirst()
    {
        var results = TweakSearchScorer.Search("DarkTheme", Tweaks);

        Assert.NotEmpty(results);
        Assert.Equal("DarkTheme", results[0].Name);
    }

    [Fact]
    public void Search_PartialDescriptionMatch_FindsTweak()
    {
        var results = TweakSearchScorer.Search("telemetry", Tweaks);

        Assert.Contains(results, tweak => tweak.Name == "DiagTrackService");
    }

    [Fact]
    public void Search_NoMatch_ReturnsEmpty()
    {
        var results = TweakSearchScorer.Search("zzzznonexistentzzzz", Tweaks);

        Assert.Empty(results);
    }

    [Fact]
    public void Search_CategoryMatch_FindsTweaksInThatCategory()
    {
        var results = TweakSearchScorer.Search("Gaming", Tweaks);

        Assert.Contains(results, tweak => tweak.Name == "GameBar");
    }

    [Fact]
    public void Search_RespectsMaxResults()
    {
        var manyTweaks = Enumerable.Range(0, 10)
            .Select(i => (ITweak)new FakeTweak("System", $"Tweak{i}", "A generic system tweak."))
            .ToList();

        var results = TweakSearchScorer.Search("Tweak", manyTweaks, maxResults: 3);

        Assert.Equal(3, results.Count);
    }
}
