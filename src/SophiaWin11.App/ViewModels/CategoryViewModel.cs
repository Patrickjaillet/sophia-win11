using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SophiaWin11.Core.Abstractions;
using Wpf.Ui;

namespace SophiaWin11.App.ViewModels;

public sealed partial class CategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading = true;

    public CategoryViewModel(string category, IReadOnlyList<ITweak> tweaks, ISnackbarService snackbarService)
    {
        CategoryName = category;
        Tweaks = new ObservableCollection<TweakRowViewModel>(
            tweaks.Select(tweak => new TweakRowViewModel(tweak, snackbarService)));
    }

    public string CategoryName { get; }

    public ObservableCollection<TweakRowViewModel> Tweaks { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;

        try
        {
            await Task.WhenAll(Tweaks.Select(row => row.RefreshStateAsync(cancellationToken))).ConfigureAwait(true);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
