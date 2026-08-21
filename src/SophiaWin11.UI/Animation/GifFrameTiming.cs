namespace SophiaWin11.UI.Animation;

public static class GifFrameTiming
{
    public static TimeSpan TotalDuration(IReadOnlyList<TimeSpan> durations)
    {
        var total = TimeSpan.Zero;
        foreach (var duration in durations)
        {
            total += duration;
        }

        return total;
    }

    public static int ResolveFrameIndex(IReadOnlyList<TimeSpan> durations, TimeSpan elapsed)
    {
        if (durations.Count == 0)
        {
            return -1;
        }

        var total = TotalDuration(durations);
        if (total <= TimeSpan.Zero)
        {
            return 0;
        }

        var normalizedElapsed = elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
        var wrapped = TimeSpan.FromTicks(normalizedElapsed.Ticks % total.Ticks);

        var accumulated = TimeSpan.Zero;
        for (var index = 0; index < durations.Count; index++)
        {
            accumulated += durations[index];
            if (wrapped < accumulated)
            {
                return index;
            }
        }

        return durations.Count - 1;
    }
}
