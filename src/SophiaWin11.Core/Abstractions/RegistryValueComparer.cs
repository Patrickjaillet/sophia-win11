namespace SophiaWin11.Core.Abstractions;

public static class RegistryValueComparer
{
    public static bool AreEqual(object? first, object? second)
    {
        if (first is byte[] firstBytes && second is byte[] secondBytes)
        {
            return firstBytes.AsSpan().SequenceEqual(secondBytes);
        }

        if (first is string[] firstStrings && second is string[] secondStrings)
        {
            return firstStrings.AsSpan().SequenceEqual(secondStrings);
        }

        return Equals(first, second);
    }
}
