using SophiaWin11.App.ViewModels;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Tests.Fakes;
using Xunit;

namespace SophiaWin11.Tests.ViewModels;

public sealed class CategoryViewModelTests
{
    [Fact]
    public async Task LoadAsync_RefreshesAppliedStateForEveryRow()
    {
        var tweaks = new List<ITweak>
        {
            new FakeTweak("Gaming", "A", "descA", initiallyApplied: true),
            new FakeTweak("Gaming", "B", "descB", initiallyApplied: false),
        };
        var viewModel = new CategoryViewModel("Gaming", tweaks, new FakeSnackbarService(), new FakeSessionService(), new FakeLocalizationService());

        await viewModel.LoadAsync();

        Assert.False(viewModel.IsLoading);
        Assert.True(viewModel.Tweaks[0].IsApplied);
        Assert.False(viewModel.Tweaks[1].IsApplied);
    }

    [Fact]
    public async Task ApplyCommand_CallsApplyAndUpdatesRowState()
    {
        var tweak = new FakeTweak("Gaming", "A", "descA", initiallyApplied: false);
        var viewModel = new CategoryViewModel("Gaming", [tweak], new FakeSnackbarService(), new FakeSessionService(), new FakeLocalizationService());
        await viewModel.LoadAsync();

        await viewModel.Tweaks[0].ApplyCommand.ExecuteAsync(null);

        Assert.Equal(1, tweak.ApplyCallCount);
        Assert.True(viewModel.Tweaks[0].IsApplied);
    }

    [Fact]
    public async Task RevertCommand_CallsRevertAndUpdatesRowState()
    {
        var tweak = new FakeTweak("Gaming", "A", "descA", initiallyApplied: true);
        var viewModel = new CategoryViewModel("Gaming", [tweak], new FakeSnackbarService(), new FakeSessionService(), new FakeLocalizationService());
        await viewModel.LoadAsync();

        await viewModel.Tweaks[0].RevertCommand.ExecuteAsync(null);

        Assert.Equal(1, tweak.RevertCallCount);
        Assert.False(viewModel.Tweaks[0].IsApplied);
    }

    [Fact]
    public async Task ApplyCommand_WhenTweakThrows_ShowsFailureSnackbarAndDoesNotCrash()
    {
        var tweak = new FakeTweak("Gaming", "A", "descA") { ThrowOnApply = true };
        var snackbar = new FakeSnackbarService();
        var viewModel = new CategoryViewModel("Gaming", [tweak], snackbar, new FakeSessionService(), new FakeLocalizationService());
        await viewModel.LoadAsync();

        await viewModel.Tweaks[0].ApplyCommand.ExecuteAsync(null);

        Assert.False(viewModel.Tweaks[0].IsApplied);
        Assert.Single(snackbar.Shown);
    }

    [Fact]
    public async Task ApplyCommand_RoutesThroughSessionService()
    {
        var tweak = new FakeTweak("Gaming", "A", "descA", initiallyApplied: false);
        var sessionService = new FakeSessionService();
        var viewModel = new CategoryViewModel("Gaming", [tweak], new FakeSnackbarService(), sessionService, new FakeLocalizationService());
        await viewModel.LoadAsync();

        await viewModel.Tweaks[0].ApplyCommand.ExecuteAsync(null);

        Assert.Single(sessionService.ApplyCalls);
        Assert.Same(tweak, sessionService.ApplyCalls[0][0]);
        Assert.True(viewModel.Tweaks[0].IsApplied);
    }

    [Fact]
    public async Task RevertCommand_AfterApply_RollsBackTheAppliedSession()
    {
        var tweak = new FakeTweak("Gaming", "A", "descA", initiallyApplied: false);
        var sessionService = new FakeSessionService();
        var viewModel = new CategoryViewModel("Gaming", [tweak], new FakeSnackbarService(), sessionService, new FakeLocalizationService());
        await viewModel.LoadAsync();

        await viewModel.Tweaks[0].ApplyCommand.ExecuteAsync(null);
        await viewModel.Tweaks[0].RevertCommand.ExecuteAsync(null);

        Assert.Single(sessionService.RollbackCalls);
        Assert.False(viewModel.Tweaks[0].IsApplied);
    }

    [Fact]
    public async Task ApplyCommand_WhenSessionServiceThrowsConflict_ShowsCautionSnackbarAndDoesNotCrash()
    {
        var tweak = new FakeTweak("Gaming", "A", "descA", initiallyApplied: false);
        var other = new FakeTweak("Gaming", "B", "descB");
        var sessionService = new FakeSessionService
        {
            ConflictsToThrow = [new TweakConflict(tweak, other, "mutually exclusive")],
        };
        var snackbar = new FakeSnackbarService();
        var viewModel = new CategoryViewModel("Gaming", [tweak], snackbar, sessionService, new FakeLocalizationService());
        await viewModel.LoadAsync();

        await viewModel.Tweaks[0].ApplyCommand.ExecuteAsync(null);

        Assert.False(viewModel.Tweaks[0].IsApplied);
        Assert.Single(snackbar.Shown);
        Assert.Contains("Conflict", snackbar.Shown[0].Title);
    }

    [Fact]
    public async Task PreviewCommand_ShowsSnackbarWithPreviewText()
    {
        var tweak = new FakeTweak("Gaming", "A", "descA");
        var snackbar = new FakeSnackbarService();
        var viewModel = new CategoryViewModel("Gaming", [tweak], snackbar, new FakeSessionService(), new FakeLocalizationService());
        await viewModel.LoadAsync();

        await viewModel.Tweaks[0].PreviewCommand.ExecuteAsync(null);

        Assert.Single(snackbar.Shown);
    }
}
