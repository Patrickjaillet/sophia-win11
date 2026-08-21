using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Xunit;

namespace SophiaWin11.Tests.Theme;

public sealed class ContrastTests
{
    private static readonly IReadOnlyDictionary<string, string> Palette = LoadDesignTokenColors();

    public static IEnumerable<object[]> NormalTextPairs()
    {
        yield return new object[] { "ColorAccentGold", "ColorBackgroundDeep", "Gold headline on deep black" };
        yield return new object[] { "ColorAccentGold", "ColorBackgroundCharcoal", "Gold headline on charcoal card" };
        yield return new object[] { "ColorTextPrimary", "ColorBackgroundDeep", "Primary ivory body text on deep black" };
        yield return new object[] { "ColorTextPrimary", "ColorBackgroundCharcoal", "Primary ivory body text on charcoal card" };
        yield return new object[] { "ColorTextSecondary", "ColorBackgroundCharcoal", "Secondary muted text on charcoal card" };
        yield return new object[] { "ColorTextSecondary", "ColorBackgroundDeep", "Secondary muted text on deep black" };
        yield return new object[] { "ColorTextOnAccent", "ColorAccentEmerald", "White badge text on emerald (Low risk)" };
        yield return new object[] { "ColorTextOnAccent", "ColorAccentBordeaux", "White badge text on bordeaux (High risk)" };
        yield return new object[] { "ColorTextOnAccent", "ColorAccentPeacock", "White badge text on peacock (restart required)" };
    }

    public static IEnumerable<object[]> LargeTextPairs()
    {
        yield return new object[] { "ColorAccentGold", "ColorBackgroundDeep", "Gold display title on deep black" };
        yield return new object[] { "ColorAccentGold", "ColorBackgroundCharcoal", "Gold display title on charcoal card" };
    }

    [Theory]
    [MemberData(nameof(NormalTextPairs))]
    public void NormalText_MeetsWcagAa_4_5To1(string foregroundKey, string backgroundKey, string description)
    {
        var ratio = ComputeContrastRatio(foregroundKey, backgroundKey);

        Assert.True(
            ratio >= 4.5,
            $"{description}: contrast ratio {ratio:F2}:1 is below the WCAG AA 4.5:1 threshold for normal text ({foregroundKey} on {backgroundKey}).");
    }

    [Theory]
    [MemberData(nameof(LargeTextPairs))]
    public void LargeText_MeetsWcagAa_3To1(string foregroundKey, string backgroundKey, string description)
    {
        var ratio = ComputeContrastRatio(foregroundKey, backgroundKey);

        Assert.True(
            ratio >= 3.0,
            $"{description}: contrast ratio {ratio:F2}:1 is below the WCAG AA 3:1 threshold for large/bold text ({foregroundKey} on {backgroundKey}).");
    }

    private static double ComputeContrastRatio(string foregroundKey, string backgroundKey)
    {
        var foreground = ParseHexColor(Palette[foregroundKey]);
        var background = ParseHexColor(Palette[backgroundKey]);

        var luminanceForeground = RelativeLuminance(foreground);
        var luminanceBackground = RelativeLuminance(background);

        var lighter = Math.Max(luminanceForeground, luminanceBackground);
        var darker = Math.Min(luminanceForeground, luminanceBackground);

        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double RelativeLuminance((byte R, byte G, byte B) color)
    {
        double Linearize(byte channel)
        {
            var c = channel / 255.0;
            return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        var r = Linearize(color.R);
        var g = Linearize(color.G);
        var b = Linearize(color.B);

        return (0.2126 * r) + (0.7152 * g) + (0.0722 * b);
    }

    private static (byte R, byte G, byte B) ParseHexColor(string hex)
    {
        var value = hex.TrimStart('#');
        if (value.Length == 8)
        {
            value = value[2..];
        }

        var r = System.Convert.ToByte(value[..2], 16);
        var g = System.Convert.ToByte(value[2..4], 16);
        var b = System.Convert.ToByte(value[4..6], 16);

        return (r, g, b);
    }

    private static IReadOnlyDictionary<string, string> LoadDesignTokenColors()
    {
        var path = GetDesignTokensPath();
        var document = XDocument.Load(path);

        XNamespace presentationNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        var colors = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var element in document.Descendants(presentationNamespace + "Color"))
        {
            var key = element.Attribute(xamlNamespace + "Key")?.Value;
            var value = element.Value.Trim();

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                colors[key] = value;
            }
        }

        return colors;
    }

    private static string GetDesignTokensPath([CallerFilePath] string testFilePath = "")
    {
        var testDirectory = Path.GetDirectoryName(testFilePath)!;
        var path = Path.GetFullPath(Path.Combine(testDirectory, "..", "..", "SophiaWin11.UI", "Theme", "DesignTokens.xaml"));

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"DesignTokens.xaml not found at expected path: {path}");
        }

        return path;
    }
}
