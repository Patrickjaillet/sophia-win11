using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SophiaWin11.Core.Abstractions;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SophiaWin11.App.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject
{
    private readonly ITweakService _tweakService;
    private readonly IProfileService _profileService;
    private readonly ISnackbarService _snackbarService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private int totalTweaksCount;

    [ObservableProperty]
    private int appliedTweaksCount;

    [ObservableProperty]
    private string lastSessionSummary;

    public DashboardViewModel(ITweakService tweakService, IProfileService profileService, ISnackbarService snackbarService, ISessionService sessionService, ILocalizationService localizationService)
    {
        _tweakService = tweakService;
        _profileService = profileService;
        _snackbarService = snackbarService;
        _sessionService = sessionService;
        _localizationService = localizationService;

        lastSessionSummary = _localizationService.GetString("Dashboard_LastSessionSummaryDefault");
        TotalTweaksCount = _tweakService.TweakCount;

        _ = RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var tweaks = _tweakService.Tweaks;
            TotalTweaksCount = tweaks.Count;

            var appliedFlags = await Task.WhenAll(
                tweaks.Select(tweak => SafeIsAppliedAsync(tweak))).ConfigureAwait(true);

            AppliedTweaksCount = appliedFlags.Count(applied => applied);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        var dialog = new SaveFileDialog
        {
            Filter = $"{_localizationService.GetString("Dashboard_ProfileFileTypeName")} (*.sophiaprofile)|*.sophiaprofile",
            FileName = "profile.sophiaprofile",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var tweaks = _tweakService.Tweaks;
            var appliedFlags = await Task.WhenAll(
                tweaks.Select(async tweak => (Tweak: tweak, Applied: await SafeIsAppliedAsync(tweak).ConfigureAwait(true)))).ConfigureAwait(true);

            var appliedIds = appliedFlags.Where(entry => entry.Applied).Select(entry => entry.Tweak.Id).ToList();
            var profileName = Path.GetFileNameWithoutExtension(dialog.FileName);
            var profile = new TweakProfile(profileName, DateTimeOffset.UtcNow, appliedIds);

            await _profileService.SaveProfileAsync(dialog.FileName, profile).ConfigureAwait(true);

            _snackbarService.Show(
                _localizationService.GetString("Dashboard_SnackbarProfileSavedTitle"),
                string.Format(_localizationService.GetString("Dashboard_SnackbarProfileSavedMessage"), appliedIds.Count, Path.GetFileName(dialog.FileName)),
                ControlAppearance.Success,
                TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            _snackbarService.Show(
                _localizationService.GetString("Dashboard_SnackbarProfileSaveFailedTitle"),
                ex.Message,
                ControlAppearance.Danger,
                TimeSpan.FromSeconds(5));
        }
    }

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = $"{_localizationService.GetString("Dashboard_ProfileFileTypeName")} (*.sophiaprofile)|*.sophiaprofile",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var profile = await _profileService.LoadProfileAsync(dialog.FileName).ConfigureAwait(true);
            var catalogById = _tweakService.Tweaks.ToDictionary(tweak => tweak.Id);

            var matchedTweaks = profile.TweakIds
                .Select(tweakId => catalogById.GetValueOrDefault(tweakId))
                .Where(tweak => tweak is not null)
                .Select(tweak => tweak!)
                .ToList();

            var appliedCount = 0;

            if (matchedTweaks.Count > 0)
            {
                var session = await _sessionService
                    .ApplySessionAsync(matchedTweaks, $"Profile load: {profile.Name}")
                    .ConfigureAwait(true);
                appliedCount = session.AppliedTweaks.Count;
            }

            await RefreshAsync().ConfigureAwait(true);

            _snackbarService.Show(
                _localizationService.GetString("Dashboard_SnackbarProfileLoadedTitle"),
                string.Format(_localizationService.GetString("Dashboard_SnackbarProfileLoadedMessage"), appliedCount, profile.TweakIds.Count, profile.Name),
                ControlAppearance.Success,
                TimeSpan.FromSeconds(3));
        }
        catch (TweakConflictException ex)
        {
            _snackbarService.Show(
                _localizationService.GetString("Dashboard_SnackbarProfileConflictTitle"),
                ex.Message,
                ControlAppearance.Caution,
                TimeSpan.FromSeconds(6));
        }
        catch (Exception ex)
        {
            _snackbarService.Show(
                _localizationService.GetString("Dashboard_SnackbarProfileLoadFailedTitle"),
                ex.Message,
                ControlAppearance.Danger,
                TimeSpan.FromSeconds(5));
        }
    }

    private static async Task<bool> SafeIsAppliedAsync(ITweak tweak)
    {
        try
        {
            return await tweak.IsAppliedAsync().ConfigureAwait(true);
        }
        catch
        {
            return false;
        }
    }
}
