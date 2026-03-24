using System.Collections;
using System.Globalization;

namespace JDownloader.Cli.Runtime;

internal static class HumanValueFormatter
{
    public static string ToDisplayString(object? value, string? keyHint, int maxWidth)
    {
        if (value is null)
            return string.Empty;

        if (value is byte[] bytes)
            return $"<binary> ({bytes.Length} bytes)";

        if (value is bool boolean)
            return boolean ? "yes" : "no";

        if (TryFormatBytes(value, keyHint, out var bytesValue))
            return bytesValue;

        if (value is string text)
            return Truncate(SingleLine(text), maxWidth);

        if (value is DateTimeOffset dto)
            return Truncate(dto.ToString("u", CultureInfo.InvariantCulture).TrimEnd(), maxWidth);

        if (value is DateTime dt)
            return Truncate(dt.ToString("u", CultureInfo.InvariantCulture).TrimEnd(), maxWidth);

        if (value is int or long or float or double or decimal)
            return Truncate(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty, maxWidth);

        if (HumanDataShape.TryGetDictionary(value, out _))
            return "{...}";

        if (HumanDataShape.TryGetList(value, out var list))
            return $"[{list.Count} item(s)]";

        if (value is IEnumerable and not string)
            return "[...]";

        return Truncate(SingleLine(value.ToString() ?? string.Empty), maxWidth);
    }

    private static bool TryFormatBytes(object value, string? keyHint, out string formatted)
    {
        formatted = string.Empty;
        if (string.IsNullOrWhiteSpace(keyHint))
            return false;

        var isBytes = keyHint.Contains("bytes", StringComparison.OrdinalIgnoreCase)
            || keyHint.Contains("traffic", StringComparison.OrdinalIgnoreCase);
        var isSpeed = keyHint.Equals("speed", StringComparison.OrdinalIgnoreCase);
        if (!isBytes && !isSpeed)
            return false;

        if (!TryToInt64(value, out var raw) || raw < 0)
            return false;

        formatted = isSpeed ? $"{FormatBytes(raw)}/s" : FormatBytes(raw);
        return true;
    }

    private static bool TryToInt64(object value, out long number)
    {
        switch (value)
        {
            case long longValue:
                number = longValue;
                return true;
            case int intValue:
                number = intValue;
                return true;
            case double doubleValue when doubleValue is >= long.MinValue and <= long.MaxValue:
                number = (long)doubleValue;
                return true;
            case decimal decimalValue when decimalValue is >= long.MinValue and <= long.MaxValue:
                number = (long)decimalValue;
                return true;
            case string text when long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed):
                number = parsed;
                return true;
            default:
                number = 0;
                return false;
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB", "PB"];
        double size = bytes;
        var unitIndex = 0;
        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return unitIndex == 0
            ? $"{bytes} {units[unitIndex]}"
            : $"{size:0.0} {units[unitIndex]}";
    }

    private static string SingleLine(string text)
    {
        return text.Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Trim();
    }

    private static string Truncate(string text, int maxWidth)
    {
        if (maxWidth <= 0 || text.Length <= maxWidth)
            return text;
        if (maxWidth <= 3)
            return text[..maxWidth];

        return text[..(maxWidth - 3)] + "...";
    }
}
