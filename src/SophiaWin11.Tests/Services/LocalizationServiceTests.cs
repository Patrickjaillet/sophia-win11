using System.ComponentModel;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using SophiaWin11.Core.Services;
using Xunit;

namespace SophiaWin11.Tests.Services;

public sealed class LocalizationServiceTests : IDisposable
{
    private readonly string _settingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SophiaWin11",
        "settings.json");

    private readonly string? _backedUpSettings;

    public LocalizationServiceTests()
    {
        _backedUpSettings = File.Exists(_settingsFilePath) ? File.ReadAllText(_settingsFilePath) : null;
    }

    public void Dispose()
    {
        if (_backedUpSettings is not null)
        {
            File.WriteAllText(_settingsFilePath, _backedUpSettings);
        }
        else if (File.Exists(_settingsFilePath))
        {
            File.Delete(_settingsFilePath);
        }
    }

    private static LocalizationService CreateService() => new(NullLogger<LocalizationService>.Instance);

    [Fact]
    public void SetCulture_ToSupportedCulture_UpdatesCurrentCulture()
    {
        var service = CreateService();

        service.SetCulture(CultureInfo.GetCultureInfo("fr"));

        Assert.Equal("fr", service.CurrentCulture.TwoLetterISOLanguageName);
    }

    [Fact]
    public void SetCulture_ToUnsupportedCulture_FallsBackToEnglish()
    {
        var service = CreateService();

        service.SetCulture(CultureInfo.GetCultureInfo("ja"));

        Assert.Equal("en", service.CurrentCulture.TwoLetterISOLanguageName);
    }

    [Fact]
    public void GetString_ForSupportedKey_ReturnsTranslatedValue()
    {
        var service = CreateService();
        service.SetCulture(CultureInfo.GetCultureInfo("fr"));

        var value = service.GetString("Nav_Dashboard");

        Assert.NotEqual("Nav_Dashboard", value);
        Assert.False(string.IsNullOrWhiteSpace(value));
    }

    [Fact]
    public void GetString_ForMissingKey_FallsBackToEnglishRatherThanThrowing()
    {
        var service = CreateService();
        service.SetCulture(CultureInfo.GetCultureInfo("fr"));

        var exception = Record.Exception(() => service.GetString("This_Key_Does_Not_Exist"));

        Assert.Null(exception);
    }

    [Fact]
    public void GetString_ForMissingKey_ReturnsTheKeyItselfAsLastResortFallback()
    {
        var service = CreateService();

        var value = service.GetString("This_Key_Does_Not_Exist");

        Assert.Equal("This_Key_Does_Not_Exist", value);
    }

    [Fact]
    public void SetCulture_RaisesPropertyChangedForCurrentCultureAndIndexer()
    {
        var service = CreateService();
        var raisedProperties = new List<string>();
        service.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName ?? string.Empty);

        service.SetCulture(CultureInfo.GetCultureInfo("de"));

        Assert.Contains("CurrentCulture", raisedProperties);
        Assert.Contains("Item[]", raisedProperties);
    }

    [Fact]
    public void SetCulture_ToSameCulture_DoesNotRaisePropertyChanged()
    {
        var service = CreateService();
        service.SetCulture(CultureInfo.GetCultureInfo("fr"));

        var raised = false;
        service.PropertyChanged += (_, _) => raised = true;
        service.SetCulture(CultureInfo.GetCultureInfo("fr"));

        Assert.False(raised);
    }

    [Fact]
    public void Indexer_ReflectsCurrentCultureTranslation()
    {
        var service = CreateService();
        service.SetCulture(CultureInfo.GetCultureInfo("en"));
        var englishValue = service["Nav_Search"];

        service.SetCulture(CultureInfo.GetCultureInfo("de"));
        var germanValue = service["Nav_Search"];

        Assert.NotEqual(englishValue, germanValue);
    }

    [Fact]
    public async Task SetCulture_PersistsPreferenceAcrossNewInstances()
    {
        var service = CreateService();
        service.SetCulture(CultureInfo.GetCultureInfo("ru"));

        var restarted = CreateService();
        await restarted.InitializeAsync();

        Assert.Equal("ru", restarted.CurrentCulture.TwoLetterISOLanguageName);
    }
}
