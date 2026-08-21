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
        var viewModel = new CategoryViewModel("Gaming", tweaks, new FakeSnackbarService());

        await viewModel.LoadAsync();

        Assert.False(viewModel.IsLoading);
        Assert.True(viewModel.Tweaks[0].IsApplied);
        Assert.False(viewModel.Tweaks[1].IsApplied);
    }

    [Fact]
    public async Task ApplyCommand_CallsApplyAndUpdatesRowState()
    {
        var tweak = new FakeTweak("Gaming", "A", "descA", initiallyApplied: false);
        var viewModel = new CategoryViewModel("Gaming", [tweak], new FakeSnackbarService());
        await viewModel.LoadAsync();

        await viewModel.Tweaks[0].ApplyCommand.ExecuteAsync(null);

        Assert.Equal(1, tweak.ApplyCallCount);
        Assert.True(viewModel.Tweaks[0].IsApplied);
    }

    [Fact]
    public async Task RevertCommand_CallsRevertAndUpdatesRowState()
    {
        var tweak = new FakeTweak("Gaming", "A", "descA", initiallyApplied: true);
        var viewModel = new CategoryViewModel("Gaming", [tweak], new FakeSnackbarService());
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
        var viewModel = new CategoryViewModel("Gaming", [tweak], snackbar);
        await viewModel.LoadAsync();

        await viewModel.Tweaks[0].ApplyCommand.ExecuteAsync(null);

        Assert.False(viewModel.Tweaks[0].IsApplied);
        Assert.Single(snackbar.Shown);
    }
}
