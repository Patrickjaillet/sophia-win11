using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Tweaks;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SophiaWin11.App.ViewModels;

public sealed partial class TweakRowViewModel : ObservableObject, IDisposable
{
    private readonly ISnackbarService _snackbarService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;
    private TweakSession? _appliedSession;

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

    public TweakRowViewModel(ITweak tweak, ISnackbarService snackbarService, ISessionService sessionService, ILocalizationService localizationService)
    {
        Tweak = tweak;
        _snackbarService = snackbarService;
        _sessionService = sessionService;
        _localizationService = localizationService;
        PropertyChangedEventManager.AddHandler(_localizationService, OnLocalizationPropertyChanged, string.Empty);
    }

    public ITweak Tweak { get; }

    public int Index { get; init; }

    public Guid Id => Tweak.Id;

    public string Name => Tweak.Name;

    public string Description => Tweak.Description;

    public string Category => Tweak.Category;

    public TweakRiskLevel RiskLevel => Tweak.RiskLevel;

    public string RiskLevelDisplay => _localizationService.GetString($"RiskLevel_{RiskLevel}");

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
        await ExecuteAsync(ApplyThroughSessionAsync, _localizationService.GetString("TweakRow_AppliedVerb")).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task RevertAsync()
    {
        await ExecuteAsync(RevertThroughSessionAsync, _localizationService.GetString("TweakRow_RevertedVerb")).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task PreviewAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            var preview = await Tweak.PreviewAsync().ConfigureAwait(true);

            _snackbarService.Show(
                string.Format(_localizationService.GetString("TweakRow_PreviewTitle"), Name),
                string.IsNullOrWhiteSpace(preview) ? _localizationService.GetString("TweakRow_PreviewNoChanges") : preview,
                ControlAppearance.Info,
                TimeSpan.FromSeconds(8));
        }
        catch (Exception ex)
        {
            _snackbarService.Show(
                _localizationService.GetString("TweakRow_PreviewFailedTitle"),
                string.Format(_localizationService.GetString("TweakRow_PreviewFailedMessage"), Name, ex.Message),
                ControlAppearance.Danger,
                TimeSpan.FromSeconds(5));
        }
    }

    private async Task ApplyThroughSessionAsync(CancellationToken cancellationToken)
    {
        _appliedSession = await _sessionService
            .ApplySessionAsync(new[] { Tweak }, $"Apply '{Name}'", cancellationToken)
            .ConfigureAwait(true);
    }

    private async Task RevertThroughSessionAsync(CancellationToken cancellationToken)
    {
        if (_appliedSession is not null)
        {
            await _sessionService.RollbackSessionAsync(_appliedSession, cancellationToken).ConfigureAwait(true);
            _appliedSession = null;
        }
        else
        {
            await Tweak.RevertAsync(cancellationToken).ConfigureAwait(true);
        }
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
                _localizationService.GetString("TweakRow_SuccessTitle"),
                string.Format(_localizationService.GetString("TweakRow_SuccessMessage"), Name, pastTenseVerb),
                ControlAppearance.Success,
                TimeSpan.FromSeconds(3));

            IsBusy = false;
            ShowSuccessIndicator = true;
        }
        catch (TweakConflictException ex)
        {
            _snackbarService.Show(
                _localizationService.GetString("TweakRow_ConflictTitle"),
                string.Format(_localizationService.GetString("TweakRow_ConflictMessage"), Name, ex.Message),
                ControlAppearance.Caution,
                TimeSpan.FromSeconds(6));

            IsBusy = false;
            ShowFailureIndicator = true;
        }
        catch (Exception ex)
        {
            _snackbarService.Show(
                _localizationService.GetString("TweakRow_FailedTitle"),
                string.Format(_localizationService.GetString("TweakRow_FailedMessage"), Name, ex.Message),
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

    private void OnLocalizationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ILocalizationService.CurrentCulture))
        {
            OnPropertyChanged(nameof(RiskLevelDisplay));
        }
    }

    public void Dispose()
    {
        PropertyChangedEventManager.RemoveHandler(_localizationService, OnLocalizationPropertyChanged, string.Empty);
    }
}
