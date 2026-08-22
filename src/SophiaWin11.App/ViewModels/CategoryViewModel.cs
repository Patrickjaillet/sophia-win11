using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SophiaWin11.Core.Abstractions;
using Wpf.Ui;

namespace SophiaWin11.App.ViewModels;

public sealed partial class CategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading = true;

    public CategoryViewModel(string category, IReadOnlyList<ITweak> tweaks, ISnackbarService snackbarService, ISessionService sessionService, ILocalizationService localizationService)
    {
        CategoryName = category;
        Tweaks = new ObservableCollection<TweakRowViewModel>(
            tweaks.Select((tweak, index) => new TweakRowViewModel(tweak, snackbarService, sessionService, localizationService) { Index = index }));
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
