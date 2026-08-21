namespace SophiaWin11.UI.Animation;

public static class LoopClock
{
    public static TimeSpan Advance(TimeSpan elapsed, TimeSpan delta, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        var next = elapsed + delta;
        if (next < duration)
        {
            return next;
        }

        return TimeSpan.FromTicks(next.Ticks % duration.Ticks);
    }
}
