using System.ComponentModel;
using System.Globalization;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Tests.Fakes;

public sealed class FakeLocalizationService : ILocalizationService
{
    private static readonly IReadOnlyList<CultureInfo> SupportedCultureList =
    [
        CultureInfo.GetCultureInfo("en"),
        CultureInfo.GetCultureInfo("fr"),
        CultureInfo.GetCultureInfo("de"),
        CultureInfo.GetCultureInfo("ru"),
        CultureInfo.GetCultureInfo("uk"),
    ];

    public event PropertyChangedEventHandler? PropertyChanged;

    public CultureInfo CurrentCulture { get; private set; } = CultureInfo.GetCultureInfo("en");

    public IReadOnlyList<CultureInfo> SupportedCultures => SupportedCultureList;

    public string this[string key] => GetString(key);

    public string GetString(string key) => key;

    public void SetCulture(CultureInfo culture)
    {
        CurrentCulture = culture;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
