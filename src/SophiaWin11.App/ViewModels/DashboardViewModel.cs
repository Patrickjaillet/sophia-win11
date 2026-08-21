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

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private int totalTweaksCount;

    [ObservableProperty]
    private int appliedTweaksCount;

    [ObservableProperty]
    private string lastSessionSummary = "No session yet.";

    public DashboardViewModel(ITweakService tweakService, IProfileService profileService, ISnackbarService snackbarService)
    {
        _tweakService = tweakService;
        _profileService = profileService;
        _snackbarService = snackbarService;

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
            Filter = "Sophia profile (*.sophiaprofile)|*.sophiaprofile",
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
                "Profile saved",
                $"Saved {appliedIds.Count} applied tweak(s) to '{Path.GetFileName(dialog.FileName)}'.",
                ControlAppearance.Success,
                TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            _snackbarService.Show(
                "Could not save profile",
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
            Filter = "Sophia profile (*.sophiaprofile)|*.sophiaprofile",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var profile = await _profileService.LoadProfileAsync(dialog.FileName).ConfigureAwait(true);
            var catalogById = _tweakService.Tweaks.ToDictionary(tweak => tweak.Id);

            var appliedCount = 0;

            foreach (var tweakId in profile.TweakIds)
            {
                if (catalogById.TryGetValue(tweakId, out var tweak))
                {
                    await tweak.ApplyAsync().ConfigureAwait(true);
                    appliedCount++;
                }
            }

            await RefreshAsync().ConfigureAwait(true);

            _snackbarService.Show(
                "Profile loaded",
                $"Applied {appliedCount} of {profile.TweakIds.Count} tweak(s) from '{profile.Name}'.",
                ControlAppearance.Success,
                TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            _snackbarService.Show(
                "Could not load profile",
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
