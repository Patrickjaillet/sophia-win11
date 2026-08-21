namespace SophiaWin11.Core.Abstractions;

public sealed record TweakConflict(ITweak First, ITweak Second, string Reason);

public interface IConflictDetectionService
{
    IReadOnlyList<TweakConflict> DetectConflicts(IEnumerable<ITweak> selectedTweaks);
}
