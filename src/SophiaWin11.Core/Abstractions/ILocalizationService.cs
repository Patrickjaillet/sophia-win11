using System.ComponentModel;
using System.Globalization;

namespace SophiaWin11.Core.Abstractions;

public interface ILocalizationService : INotifyPropertyChanged
{
    CultureInfo CurrentCulture { get; }

    IReadOnlyList<CultureInfo> SupportedCultures { get; }

    string this[string key] { get; }

    string GetString(string key);

    void SetCulture(CultureInfo culture);

    Task InitializeAsync(CancellationToken cancellationToken = default);
}
