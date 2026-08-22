using System.Windows.Data;
using System.Windows.Markup;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.App.Localization;

public sealed class TranslateExtension : MarkupExtension
{
    public static ILocalizationService? LocalizationService { get; set; }

    public string Key { get; set; }

    public TranslateExtension()
    {
        Key = string.Empty;
    }

    public TranslateExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (LocalizationService is null)
        {
            return Key;
        }

        var binding = new Binding($"[{Key}]")
        {
            Source = LocalizationService,
            Mode = BindingMode.OneWay,
        };

        return binding.ProvideValue(serviceProvider);
    }
}
