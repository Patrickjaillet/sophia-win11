using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SophiaWin11.Tests.Fakes;

public sealed class FakeSnackbarService : ISnackbarService
{
    public TimeSpan DefaultTimeOut { get; set; } = TimeSpan.FromSeconds(2);

    public List<(string Title, string Message, ControlAppearance Appearance)> Shown { get; } = [];

    public void SetSnackbarPresenter(SnackbarPresenter contentPresenter)
    {
    }

    public SnackbarPresenter GetSnackbarPresenter() => new();

    public void Show(string title, string message, ControlAppearance appearance, IconElement? icon, TimeSpan timeout)
    {
        Shown.Add((title, message, appearance));
    }
}
