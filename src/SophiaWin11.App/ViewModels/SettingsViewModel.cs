using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.App.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private LanguageOption? selectedLanguage;

    public SettingsViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;

        Languages = new ObservableCollection<LanguageOption>(
            _localizationService.SupportedCultures.Select(culture => new LanguageOption(culture, ResolveNativeName(culture))));

        selectedLanguage = Languages.FirstOrDefault(option =>
            string.Equals(option.Culture.TwoLetterISOLanguageName, _localizationService.CurrentCulture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase));
    }

    public ObservableCollection<LanguageOption> Languages { get; }

    partial void OnSelectedLanguageChanged(LanguageOption? value)
    {
        if (value is null)
        {
            return;
        }

        _localizationService.SetCulture(value.Culture);
    }

    private static string ResolveNativeName(CultureInfo culture) => culture.TwoLetterISOLanguageName switch
    {
        "en" => "English",
        "fr" => "Français",
        "de" => "Deutsch",
        "ru" => "Русский",
        "uk" => "Українська",
        _ => culture.NativeName,
    };

    public sealed record LanguageOption(CultureInfo Culture, string NativeName);
}
