using System.Reflection;
using System.Text.Json;
using Xunit;

namespace SophiaWin11.Tests.Catalog;

public sealed class TranslatedCatalogInvariantTests
{
    private static readonly string[] TranslatedLanguages = ["fr", "de", "ru", "uk"];
    private static readonly HashSet<string> TranslatableFields = new(StringComparer.Ordinal) { "name", "description", "category" };

    public static IEnumerable<object[]> Languages() => TranslatedLanguages.Select(language => new object[] { language });

    [Theory]
    [MemberData(nameof(Languages))]
    public void TranslatedCatalog_HasSameEntryCountAndIdOrderAsEnglish(string language)
    {
        var english = LoadTweaks("en");
        var translated = LoadTweaks(language);

        Assert.Equal(97, english.Count);
        Assert.Equal(english.Count, translated.Count);

        var englishIds = english.Select(entry => entry.GetProperty("id").GetString()).ToList();
        var translatedIds = translated.Select(entry => entry.GetProperty("id").GetString()).ToList();

        Assert.Equal(englishIds, translatedIds);
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void TranslatedCatalog_PreservesEveryNonTranslatableField(string language)
    {
        var english = LoadTweaks("en");
        var translated = LoadTweaks(language);

        for (var index = 0; index < english.Count; index++)
        {
            var englishEntry = english[index];
            var translatedEntry = translated[index];
            var id = englishEntry.GetProperty("id").GetString();

            foreach (var property in englishEntry.EnumerateObject())
            {
                if (TranslatableFields.Contains(property.Name))
                {
                    continue;
                }

                Assert.True(
                    translatedEntry.TryGetProperty(property.Name, out var translatedValue),
                    $"[{language}] entry {index} (id={id}) is missing non-translatable field '{property.Name}'.");

                Assert.True(
                    JsonElementDeepEquals(property.Value, translatedValue),
                    $"[{language}] entry {index} (id={id}) field '{property.Name}' was altered by translation.");
            }

            foreach (var property in translatedEntry.EnumerateObject())
            {
                if (TranslatableFields.Contains(property.Name))
                {
                    continue;
                }

                Assert.True(
                    englishEntry.TryGetProperty(property.Name, out _),
                    $"[{language}] entry {index} (id={id}) has an unexpected extra field '{property.Name}' not present in the English catalog.");
            }
        }
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void TranslatedCatalog_UsesConsistentCategoryTranslation(string language)
    {
        var english = LoadTweaks("en");
        var translated = LoadTweaks(language);

        var categoryMap = new Dictionary<string, string>(StringComparer.Ordinal);

        for (var index = 0; index < english.Count; index++)
        {
            var englishCategory = english[index].GetProperty("category").GetString()!;
            var translatedCategory = translated[index].GetProperty("category").GetString()!;

            if (categoryMap.TryGetValue(englishCategory, out var expected))
            {
                Assert.Equal(expected, translatedCategory);
            }
            else
            {
                categoryMap[englishCategory] = translatedCategory;
            }
        }

        Assert.Equal(6, categoryMap.Count);
    }

    private static List<JsonElement> LoadTweaks(string language)
    {
        var assembly = Assembly.Load("SophiaWin11.Core");
        var resourceName = $"SophiaWin11.Core.Catalog.tweaks.{language}.json";
        using var stream = assembly.GetManifestResourceStream(resourceName)
                            ?? throw new InvalidOperationException($"Embedded catalog resource '{resourceName}' not found.");
        using var document = JsonDocument.Parse(stream);
        return document.RootElement.GetProperty("tweaks").EnumerateArray().Select(element => element.Clone()).ToList();
    }

    private static bool JsonElementDeepEquals(JsonElement a, JsonElement b)
    {
        if (a.ValueKind != b.ValueKind)
        {
            return false;
        }

        return a.ValueKind switch
        {
            JsonValueKind.Object => JsonObjectsEqual(a, b),
            JsonValueKind.Array => JsonArraysEqual(a, b),
            JsonValueKind.String => a.GetString() == b.GetString(),
            JsonValueKind.Number => a.GetDecimal() == b.GetDecimal(),
            JsonValueKind.True or JsonValueKind.False => a.GetBoolean() == b.GetBoolean(),
            JsonValueKind.Null or JsonValueKind.Undefined => true,
            _ => a.GetRawText() == b.GetRawText(),
        };
    }

    private static bool JsonObjectsEqual(JsonElement a, JsonElement b)
    {
        var aProperties = a.EnumerateObject().ToDictionary(property => property.Name, property => property.Value, StringComparer.Ordinal);
        var bProperties = b.EnumerateObject().ToDictionary(property => property.Name, property => property.Value, StringComparer.Ordinal);

        if (aProperties.Count != bProperties.Count)
        {
            return false;
        }

        return aProperties.All(pair =>
            bProperties.TryGetValue(pair.Key, out var otherValue) && JsonElementDeepEquals(pair.Value, otherValue));
    }

    private static bool JsonArraysEqual(JsonElement a, JsonElement b)
    {
        var aItems = a.EnumerateArray().ToList();
        var bItems = b.EnumerateArray().ToList();

        if (aItems.Count != bItems.Count)
        {
            return false;
        }

        return !aItems.Where((item, index) => !JsonElementDeepEquals(item, bItems[index])).Any();
    }
}
