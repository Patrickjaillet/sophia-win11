using SophiaWin11.App.ViewModels;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Tests.Fakes;
using Xunit;

namespace SophiaWin11.Tests.ViewModels;

public sealed class SearchViewModelTests
{
    private static FakeTweakService CreateTweakService() => new(
    [
        new FakeTweak("Privacy & Telemetry", "DiagTrackService", "Disable telemetry collection."),
        new FakeTweak("UI & Personalization", "DarkTheme", "Enable the dark theme."),
        new FakeTweak("Gaming", "GameBar", "Disable the Xbox Game Bar overlay."),
    ]);

    [Fact]
    public void Construction_EmptyQuery_ShowsAllTweaks()
    {
        var viewModel = new SearchViewModel(CreateTweakService(), new FakeSnackbarService(), new FakeSessionService(), new FakeLocalizationService());

        Assert.Equal(3, viewModel.Results.Count);
    }

    [Fact]
    public void SettingQuery_FiltersResultsLive()
    {
        var viewModel = new SearchViewModel(CreateTweakService(), new FakeSnackbarService(), new FakeSessionService(), new FakeLocalizationService())
        {
            Query = "theme",
        };

        Assert.Single(viewModel.Results);
        Assert.Equal("DarkTheme", viewModel.Results[0].Name);
    }

    [Fact]
    public void SettingQuery_NoMatches_ClearsResults()
    {
        var viewModel = new SearchViewModel(CreateTweakService(), new FakeSnackbarService(), new FakeSessionService(), new FakeLocalizationService())
        {
            Query = "doesnotexistanywhere",
        };

        Assert.Empty(viewModel.Results);
    }

    [Fact]
    public void ClearingQuery_RestoresAllResults()
    {
        var viewModel = new SearchViewModel(CreateTweakService(), new FakeSnackbarService(), new FakeSessionService(), new FakeLocalizationService())
        {
            Query = "theme",
        };
        Assert.Single(viewModel.Results);

        viewModel.Query = string.Empty;

        Assert.Equal(3, viewModel.Results.Count);
    }
}
