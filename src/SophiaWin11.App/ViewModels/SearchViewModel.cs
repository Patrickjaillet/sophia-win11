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

    [ObservableProperty]
    private string query = string.Empty;

    public SearchViewModel(ITweakService tweakService, ISnackbarService snackbarService)
    {
        _tweakService = tweakService;
        _snackbarService = snackbarService;
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
            Results.Add(new TweakRowViewModel(tweak, _snackbarService));
        }

        _ = RefreshAppliedStateAsync();
    }

    private async Task RefreshAppliedStateAsync()
    {
        var rows = Results.ToList();
        await Task.WhenAll(rows.Select(row => row.RefreshStateAsync())).ConfigureAwait(true);
    }
}
