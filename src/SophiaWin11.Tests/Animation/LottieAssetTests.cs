using System.IO;
using System.Runtime.CompilerServices;
using Xunit;
using SkottieAnimation = SkiaSharp.Skottie.Animation;

namespace SophiaWin11.Tests.Animation;

public sealed class LottieAssetTests
{
    public static IEnumerable<object[]> AssetFileNames()
    {
        yield return new object[] { "splash-loop.json" };
        yield return new object[] { "status-success.json" };
        yield return new object[] { "status-failure.json" };
        yield return new object[] { "status-progress.json" };
        yield return new object[] { "loading-mascot.json" };
        yield return new object[] { "about-banner.json" };
    }

    [Theory]
    [MemberData(nameof(AssetFileNames))]
    public void Asset_ParsesAndReportsSaneDuration(string fileName)
    {
        var path = GetAssetPath(fileName);

        using var stream = File.OpenRead(path);
        using var animation = SkottieAnimation.Create(stream);

        Assert.NotNull(animation);
        Assert.True(animation!.Duration.TotalMilliseconds > 0);
        Assert.True(animation.Duration.TotalSeconds <= 10);
        Assert.True(animation.Fps > 0);
        Assert.True(animation.InPoint <= animation.OutPoint);
    }

    [Fact]
    public void TotalAssetSize_StaysUnderFiveMegabytes()
    {
        var directory = Path.GetDirectoryName(GetAssetPath("splash-loop.json"))!;
        var totalBytes = Directory.GetFiles(directory, "*.json").Sum(f => new FileInfo(f).Length);

        Assert.True(totalBytes < 5 * 1024 * 1024, $"Animated assets total {totalBytes} bytes, expected under 5 MB.");
    }

    private static string GetAssetPath(string fileName, [CallerFilePath] string testFilePath = "")
    {
        var testDirectory = Path.GetDirectoryName(testFilePath)!;
        var path = Path.GetFullPath(Path.Combine(testDirectory, "..", "..", "SophiaWin11.UI", "Assets", "Animations", fileName));

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Lottie asset not found at expected path: {path}");
        }

        return path;
    }
}
