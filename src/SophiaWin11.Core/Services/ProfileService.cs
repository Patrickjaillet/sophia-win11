using System.Text.Json;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class ProfileService : IProfileService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public async Task SaveProfileAsync(string path, TweakProfile profile, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(profile, SerializerOptions);
        await File.WriteAllTextAsync(path, json, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TweakProfile> LoadProfileAsync(string path, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<TweakProfile>(json, SerializerOptions)
               ?? throw new InvalidOperationException($"Profile file '{path}' could not be parsed.");
    }
}
