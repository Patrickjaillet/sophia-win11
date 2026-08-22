using SophiaWin11.Core.Services;
using Xunit;

namespace SophiaWin11.Tests.Services;

public sealed class AppVersionFormatterTests
{
    [Theory]
    [InlineData(1, 2, 0, 0, "v1.2.0.0")]
    [InlineData(1, 1, 0, 0, "v1.1.0.0")]
    [InlineData(2, 0, 15, 3, "v2.0.15.3")]
    public void ToDisplayString_FormatsAllFourSemVerSegments(int major, int minor, int build, int revision, string expected)
    {
        var version = new Version(major, minor, build, revision);

        var result = AppVersionFormatter.ToDisplayString(version);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToDisplayString_NegativeBuildAndRevision_ClampToZero()
    {
        var version = new Version(1, 0);

        var result = AppVersionFormatter.ToDisplayString(version);

        Assert.Equal("v1.0.0.0", result);
    }
}
