namespace ESPresenseHelper;

public static class StringExtensions
{
    public static string RedactDeviceId(this string str, int showFirstN = 3, int showLastN = 3, int minRedacted = 3)
    {
        if (str.Contains(':', StringComparison.Ordinal))
        {
            var parts = str.Split(':', 2);
            return $"{parts[0]}:{Redact(parts[1], showFirstN, showLastN, minRedacted)}";
        }

        return Redact(str, showFirstN, showLastN, minRedacted);
    }

    public static string Redact(this string fragment, int showFirstN, int showLastN, int minRedacted)
    {
        if (fragment.Length < showFirstN + minRedacted + showLastN)
        {
            return new string('*', showFirstN + showLastN);
        }

        var left = fragment.Substring(0, showFirstN);
        var right = fragment.Substring(fragment.Length - showLastN);
        return left + new string('*', minRedacted) + right;
    }
}
