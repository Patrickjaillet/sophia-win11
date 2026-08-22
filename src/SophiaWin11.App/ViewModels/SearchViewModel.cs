using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SophiaWin11.App.Services;
using SophiaWin11.Core.Abstractions;
using Wpf.Ui;

namespace SophiaWin11.App.ViewModels;

public sealed partial class SearchViewModel : ObservableObject
{
    private const int MaxResults = 50;

    private readonly ITweakService _tweakService;
    private readonly ISnackbarService _snackbarService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private string query = string.Empty;

    public SearchViewModel(ITweakService tweakService, ISnackbarService snackbarService, ISessionService sessionService, ILocalizationService localizationService)
    {
        _tweakService = tweakService;
        _snackbarService = snackbarService;
        _sessionService = sessionService;
        _localizationService = localizationService;
        Results = new ObservableCollection<TweakRowViewModel>();
        RefreshResults();
    }

    public ObservableCollection<TweakRowViewModel> Results { get; }

    partial void OnQueryChanged(string value) => RefreshResults();

    private void RefreshResults()
    {
        var matches = TweakSearchScorer.Search(Query, _tweakService.Tweaks, MaxResults);

        Results.Clear();

        foreach (var tweak in matches)
        {
            Results.Add(new TweakRowViewModel(tweak, _snackbarService, _sessionService, _localizationService));
        }

        _ = RefreshAppliedStateAsync();
    }

    private async Task RefreshAppliedStateAsync()
    {
        var rows = Results.ToList();
        await Task.WhenAll(rows.Select(row => row.RefreshStateAsync())).ConfigureAwait(true);
    }
}
