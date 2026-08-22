using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Tweaks;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SophiaWin11.App.ViewModels;

public sealed partial class TweakRowViewModel : ObservableObject
{
    private readonly ISnackbarService _snackbarService;

    [ObservableProperty]
    private bool isApplied;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowFastProgressIndicator))]
    [NotifyPropertyChangedFor(nameof(ShowLongRunningProgressIndicator))]
    private bool isBusy;

    [ObservableProperty]
    private bool showSuccessIndicator;

    [ObservableProperty]
    private bool showFailureIndicator;

    [ObservableProperty]
    private bool showActionButtons = true;

    private CancellationTokenSource? resultResetCts;

    public TweakRowViewModel(ITweak tweak, ISnackbarService snackbarService)
    {
        Tweak = tweak;
        _snackbarService = snackbarService;
    }

    public ITweak Tweak { get; }

    public int Index { get; init; }

    public Guid Id => Tweak.Id;

    public string Name => Tweak.Name;

    public string Description => Tweak.Description;

    public string Category => Tweak.Category;

    public TweakRiskLevel RiskLevel => Tweak.RiskLevel;

    public bool RequiresRestart => Tweak.RequiresRestart;

    public bool IsLongRunningOperation => Tweak is PowerShellNativeTweak;

    public bool ShowFastProgressIndicator => IsBusy && !IsLongRunningOperation;

    public bool ShowLongRunningProgressIndicator => IsBusy && IsLongRunningOperation;

    public async Task RefreshStateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsApplied = await Tweak.IsAppliedAsync(cancellationToken).ConfigureAwait(true);
        }
        catch
        {
            IsApplied = false;
        }
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        await ExecuteAsync(Tweak.ApplyAsync, "applied").ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task RevertAsync()
    {
        await ExecuteAsync(Tweak.RevertAsync, "reverted").ConfigureAwait(true);
    }

    private async Task ExecuteAsync(Func<CancellationToken, Task> action, string pastTenseVerb)
    {
        if (IsBusy)
        {
            return;
        }

        resultResetCts?.Cancel();
        IsBusy = true;
        ShowSuccessIndicator = false;
        ShowFailureIndicator = false;
        ShowActionButtons = false;

        try
        {
            await action(CancellationToken.None).ConfigureAwait(true);
            await RefreshStateAsync().ConfigureAwait(true);

            _snackbarService.Show(
                "Success",
                $"'{Name}' was {pastTenseVerb}.",
                ControlAppearance.Success,
                TimeSpan.FromSeconds(3));

            IsBusy = false;
            ShowSuccessIndicator = true;
        }
        catch (Exception ex)
        {
            _snackbarService.Show(
                "Failed",
                $"Could not apply '{Name}': {ex.Message}",
                ControlAppearance.Danger,
                TimeSpan.FromSeconds(5));

            IsBusy = false;
            ShowFailureIndicator = true;
        }

        ScheduleResultReset();
    }

    private void ScheduleResultReset()
    {
        resultResetCts = new CancellationTokenSource();
        _ = ResetResultIndicatorsAsync(resultResetCts.Token);
    }

    private async Task ResetResultIndicatorsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(2.5), cancellationToken).ConfigureAwait(true);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        ShowSuccessIndicator = false;
        ShowFailureIndicator = false;
        ShowActionButtons = true;
    }
}
