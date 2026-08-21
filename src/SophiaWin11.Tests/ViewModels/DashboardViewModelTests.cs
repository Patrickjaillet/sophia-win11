using SophiaWin11.App.ViewModels;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Services;
using SophiaWin11.Tests.Fakes;
using Xunit;

namespace SophiaWin11.Tests.ViewModels;

public sealed class DashboardViewModelTests
{
    [Fact]
    public async Task Construction_ComputesAppliedAndTotalCounts()
    {
        var tweaks = new List<ITweak>
        {
            new FakeTweak("Privacy & Telemetry", "A", "descA", initiallyApplied: true),
            new FakeTweak("Privacy & Telemetry", "B", "descB", initiallyApplied: false),
            new FakeTweak("System", "C", "descC", initiallyApplied: true),
        };
        var tweakService = new FakeTweakService(tweaks);
        var viewModel = new DashboardViewModel(tweakService, new ProfileService(), new FakeSnackbarService());

        await WaitForConditionAsync(() => !viewModel.IsLoading);

        Assert.Equal(3, viewModel.TotalTweaksCount);
        Assert.Equal(2, viewModel.AppliedTweaksCount);
    }

    [Fact]
    public async Task RefreshCommand_RecomputesCountsAfterExternalApply()
    {
        var tweaks = new List<ITweak>
        {
            new FakeTweak("System", "A", "descA", initiallyApplied: false),
        };
        var tweakService = new FakeTweakService(tweaks);
        var viewModel = new DashboardViewModel(tweakService, new ProfileService(), new FakeSnackbarService());

        await WaitForConditionAsync(() => !viewModel.IsLoading);
        Assert.Equal(0, viewModel.AppliedTweaksCount);

        await tweaks[0].ApplyAsync();
        await viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.Equal(1, viewModel.AppliedTweaksCount);
    }

    private static async Task WaitForConditionAsync(Func<bool> condition)
    {
        var attempts = 0;
        while (!condition() && attempts < 100)
        {
            await Task.Delay(10);
            attempts++;
        }
    }
}
