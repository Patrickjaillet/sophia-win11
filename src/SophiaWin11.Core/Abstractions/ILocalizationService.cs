using System.Globalization;

namespace SophiaWin11.Core.Abstractions;

public interface ILocalizationService
{
    CultureInfo CurrentCulture { get; }

    string GetString(string key);

    void SetCulture(CultureInfo culture);
}
