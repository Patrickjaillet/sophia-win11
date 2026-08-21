namespace SophiaWin11.Core.Abstractions;

public sealed record TweakProfile(string Name, DateTimeOffset CreatedAt, IReadOnlyList<Guid> TweakIds);

public interface IProfileService
{
    Task SaveProfileAsync(string path, TweakProfile profile, CancellationToken cancellationToken = default);

    Task<TweakProfile> LoadProfileAsync(string path, CancellationToken cancellationToken = default);
}
