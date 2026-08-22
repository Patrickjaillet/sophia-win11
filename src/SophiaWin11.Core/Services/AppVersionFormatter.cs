namespace SophiaWin11.Core.Services;

public static class AppVersionFormatter
{
    public static string ToDisplayString(Version version) =>
        $"v{version.Major}.{version.Minor}.{Math.Max(version.Build, 0)}.{Math.Max(version.Revision, 0)}";
}
