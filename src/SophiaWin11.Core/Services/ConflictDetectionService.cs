using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

public sealed class ConflictDetectionService : IConflictDetectionService
{
    public IReadOnlyList<TweakConflict> DetectConflicts(IEnumerable<ITweak> selectedTweaks)
    {
        var tweaks = selectedTweaks.ToList();
        var conflicts = new List<TweakConflict>();

        for (var i = 0; i < tweaks.Count; i++)
        {
            for (var j = i + 1; j < tweaks.Count; j++)
            {
                var first = tweaks[i];
                var second = tweaks[j];

                if (first.Id == second.Id)
                {
                    continue;
                }

                foreach (var firstImpact in first.RegistryImpact)
                {
                    foreach (var secondImpact in second.RegistryImpact)
                    {
                        if (!TargetsSameValue(firstImpact, secondImpact))
                        {
                            continue;
                        }

                        var reason = DescribeConflict(firstImpact, secondImpact);
                        if (reason is not null)
                        {
                            conflicts.Add(new TweakConflict(first, second, reason));
                        }
                    }
                }
            }
        }

        return conflicts;
    }

    private static bool TargetsSameValue(RegistryImpact first, RegistryImpact second) =>
        first.Hive == second.Hive
        && string.Equals(first.SubKey, second.SubKey, StringComparison.OrdinalIgnoreCase)
        && string.Equals(first.ValueName, second.ValueName, StringComparison.OrdinalIgnoreCase);

    private static string? DescribeConflict(RegistryImpact first, RegistryImpact second)
    {
        var target = $"{first.Hive}\\{first.SubKey}\\{first.ValueName}";

        if (first.ApplyValue is null || second.ApplyValue is null)
        {
            return $"Both tweaks target '{target}', but at least one apply value could not be determined ahead of time.";
        }

        if (Equals(first.ApplyValue, second.ApplyValue))
        {
            return null;
        }

        return $"Both tweaks target '{target}' but would apply different values ('{first.ApplyValue}' vs '{second.ApplyValue}').";
    }
}
