using System.IO;
using SophiaWin11.Core.Abstractions;
using SophiaWin11.Core.Services;
using Xunit;

namespace SophiaWin11.Tests.Services;

public sealed class ProfileServiceTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), "SophiaWin11Tests_" + Guid.NewGuid());

    [Fact]
    public async Task SaveAndLoadProfile_RoundTrips_AllFields()
    {
        var service = new ProfileService();
        var path = Path.Combine(_tempDirectory, "test.sophiaprofile");
        var profile = new TweakProfile(
            "MyProfile",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            [Guid.NewGuid(), Guid.NewGuid()]);

        await service.SaveProfileAsync(path, profile);
        var loaded = await service.LoadProfileAsync(path);

        Assert.Equal(profile.Name, loaded.Name);
        Assert.Equal(profile.CreatedAt, loaded.CreatedAt);
        Assert.Equal(profile.TweakIds, loaded.TweakIds);
    }

    [Fact]
    public async Task SaveProfileAsync_CreatesMissingDirectory()
    {
        var service = new ProfileService();
        var path = Path.Combine(_tempDirectory, "nested", "profile.sophiaprofile");
        var profile = new TweakProfile("Nested", DateTimeOffset.UtcNow, [Guid.NewGuid()]);

        await service.SaveProfileAsync(path, profile);

        Assert.True(File.Exists(path));
    }

    [Fact]
    public async Task LoadProfileAsync_MissingFile_Throws()
    {
        Directory.CreateDirectory(_tempDirectory);
        var service = new ProfileService();
        var path = Path.Combine(_tempDirectory, "does-not-exist.sophiaprofile");

        await Assert.ThrowsAsync<FileNotFoundException>(() => service.LoadProfileAsync(path));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
