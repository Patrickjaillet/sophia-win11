using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SophiaWin11.Core.Abstractions;
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
    private bool isBusy;

    public TweakRowViewModel(ITweak tweak, ISnackbarService snackbarService)
    {
        Tweak = tweak;
        _snackbarService = snackbarService;
    }

    public ITweak Tweak { get; }

    public Guid Id => Tweak.Id;

    public string Name => Tweak.Name;

    public string Description => Tweak.Description;

    public string Category => Tweak.Category;

    public TweakRiskLevel RiskLevel => Tweak.RiskLevel;

    public bool RequiresRestart => Tweak.RequiresRestart;

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

        IsBusy = true;

        try
        {
            await action(CancellationToken.None).ConfigureAwait(true);
            await RefreshStateAsync().ConfigureAwait(true);

            _snackbarService.Show(
                "Success",
                $"'{Name}' was {pastTenseVerb}.",
                ControlAppearance.Success,
                TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            _snackbarService.Show(
                "Failed",
                $"Could not apply '{Name}': {ex.Message}",
                ControlAppearance.Danger,
                TimeSpan.FromSeconds(5));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
