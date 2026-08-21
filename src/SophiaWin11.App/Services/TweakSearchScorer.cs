using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.App.Services;

public static class TweakSearchScorer
{
    public static IReadOnlyList<ITweak> Search(string query, IReadOnlyList<ITweak> tweaks, int maxResults = 50)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return tweaks;
        }

        var trimmedQuery = query.Trim();

        var scored = new List<(ITweak Tweak, int Score)>();

        foreach (var tweak in tweaks)
        {
            var score = Score(trimmedQuery, tweak.Name, allowSubsequence: true) * 3
                        + Score(trimmedQuery, tweak.Category, allowSubsequence: true) * 2
                        + Score(trimmedQuery, tweak.Description, allowSubsequence: false);

            if (score > 0)
            {
                scored.Add((tweak, score));
            }
        }

        return scored
            .OrderByDescending(entry => entry.Score)
            .ThenBy(entry => entry.Tweak.Name, StringComparer.OrdinalIgnoreCase)
            .Take(maxResults)
            .Select(entry => entry.Tweak)
            .ToList();
    }

    private static int Score(string query, string candidate, bool allowSubsequence)
    {
        if (string.IsNullOrEmpty(candidate))
        {
            return 0;
        }

        var normalizedQuery = query.ToLowerInvariant();
        var normalizedCandidate = candidate.ToLowerInvariant();

        if (normalizedCandidate == normalizedQuery)
        {
            return 1000;
        }

        var directIndex = normalizedCandidate.IndexOf(normalizedQuery, StringComparison.Ordinal);
        if (directIndex >= 0)
        {
            return 500 - directIndex;
        }

        return allowSubsequence && IsSubsequence(normalizedQuery, normalizedCandidate) ? 100 : 0;
    }

    private static bool IsSubsequence(string query, string candidate)
    {
        var queryIndex = 0;

        foreach (var character in candidate)
        {
            if (queryIndex >= query.Length)
            {
                break;
            }

            if (character == query[queryIndex])
            {
                queryIndex++;
            }
        }

        return queryIndex == query.Length;
    }
}
