using System.Collections;
using System.Globalization;

namespace JDownloader.Cli.Runtime;

public static class HumanDataRenderer
{
    private static readonly string[] PreferredColumns =
    [
        "name",
        "url",
        "host",
        "status",
        "availability",
        "enabled",
        "running",
        "finished",
        "skipped",
        "bytesLoaded",
        "bytesTotal",
        "speed",
        "eta",
        "saveTo",
        "childCount",
        "priority",
        "valid",
        "error",
        "trafficLeft",
        "trafficMax",
        "userName",
        "validUntil",
        "description",
        "installed",
        "version",
        "id",
        "uuid",
    ];

    public static IReadOnlyList<string>? Render(object? data)
    {
        if (data is null)
            return ["(empty)"];

        if (data is byte[] bytes)
            return [$"<binary> ({bytes.Length} bytes)"];

        if (TryGetDictionary(data, out var dictionary))
            return RenderObject(dictionary);

        if (TryGetList(data, out var list))
            return RenderList(list);

        return [ToDisplayString(data, keyHint: null, maxWidth: 0)];
    }

    private static IReadOnlyList<string> RenderObject(IReadOnlyDictionary<string, object?> dictionary)
    {
        if (dictionary.Count == 0)
            return ["(empty)"];

        var lines = new List<string>(dictionary.Count);
        foreach (var (key, value) in dictionary.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
        {
            var display = ToDisplayString(value, key, maxWidth: 200);
            lines.Add($"{key}: {display}");
        }

        return lines;
    }

    private static IReadOnlyList<string> RenderList(IReadOnlyList<object?> items)
    {
        if (items.Count == 0)
            return ["(empty)"];

        var dictionaries = new List<IReadOnlyDictionary<string, object?>>(items.Count);
        foreach (var item in items)
        {
            if (item is null)
                continue;
            if (!TryGetDictionary(item, out var dictionary))
            {
                dictionaries.Clear();
                break;
            }

            dictionaries.Add(dictionary);
        }

        if (dictionaries.Count > 0)
            return RenderTable(dictionaries);

        var lines = new List<string>(items.Count);
        foreach (var item in items)
            lines.Add(ToDisplayString(item, keyHint: null, maxWidth: 0));
        return lines;
    }

    private static IReadOnlyList<string> RenderTable(IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        if (rows.Count == 0)
            return ["(empty)"];

        var availableColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < Math.Min(rows.Count, 25); i++)
        {
            foreach (var key in rows[i].Keys)
                availableColumns.Add(key);
        }

        var columns = PickColumns(availableColumns);
        if (columns.Count == 0)
            return ["(empty)"];

        var maxCellWidthByColumn = columns.ToDictionary(
            column => column,
            column => column.Equals("url", StringComparison.OrdinalIgnoreCase) ? 90 : 50,
            StringComparer.OrdinalIgnoreCase);

        var widths = columns
            .ToDictionary(column => column, column => column.Length, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            foreach (var column in columns)
            {
                row.TryGetValue(column, out var value);
                var maxCellWidth = maxCellWidthByColumn[column];
                var display = ToDisplayString(value, column, maxCellWidth);
                widths[column] = Math.Min(Math.Max(widths[column], display.Length), maxCellWidth);
            }
        }

        static string Pad(string value, int width)
        {
            if (value.Length >= width)
                return value;
            return value + new string(' ', width - value.Length);
        }

        var header = string.Join("  ", columns.Select(column => Pad(column, widths[column])));
        var separator = string.Join("  ", columns.Select(column => new string('-', widths[column])));

        var lines = new List<string>(rows.Count + 2) { header, separator };
        foreach (var row in rows)
        {
            var cells = new List<string>(columns.Count);
            foreach (var column in columns)
            {
                row.TryGetValue(column, out var value);
                var maxCellWidth = maxCellWidthByColumn[column];
                var display = ToDisplayString(value, column, maxCellWidth);
                var width = widths[column];
                cells.Add(Pad(display, width));
            }

            lines.Add(string.Join("  ", cells));
        }

        return lines;
    }

    private static List<string> PickColumns(HashSet<string> available)
    {
        const int maxColumns = 8;
        var columns = new List<string>(maxColumns);

        foreach (var preferred in PreferredColumns)
        {
            if (available.Contains(preferred))
                columns.Add(preferred);
            if (columns.Count >= maxColumns)
                return columns;
        }

        var fallback = available
            .Where(key => PreferredColumns.All(preferred => !preferred.Equals(key, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
            .Take(maxColumns - columns.Count);
        columns.AddRange(fallback);
        return columns;
    }

    private static string ToDisplayString(object? value, string? keyHint, int maxWidth)
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

        if (TryGetDictionary(value, out _))
            return "{...}";

        if (TryGetList(value, out var list))
            return $"[{list.Count} item(s)]";

        if (value is IEnumerable && value is not string)
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

        if (!TryToInt64(value, out var raw))
            return false;

        if (raw < 0)
            return false;

        var human = FormatBytes(raw);
        formatted = isSpeed ? $"{human}/s" : human;
        return true;
    }

    private static bool TryToInt64(object value, out long number)
    {
        switch (value)
        {
            case long l:
                number = l;
                return true;
            case int i:
                number = i;
                return true;
            case double d when d is >= long.MinValue and <= long.MaxValue:
                number = (long)d;
                return true;
            case decimal m when m is >= long.MinValue and <= long.MaxValue:
                number = (long)m;
                return true;
            case string s when long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed):
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

    private static bool TryGetDictionary(object value, out IReadOnlyDictionary<string, object?> dictionary)
    {
        if (value is IReadOnlyDictionary<string, object?> readOnly)
        {
            dictionary = new Dictionary<string, object?>(readOnly, StringComparer.OrdinalIgnoreCase);
            return true;
        }

        if (value is IDictionary<string, object?> dict)
        {
            dictionary = new Dictionary<string, object?>(dict, StringComparer.OrdinalIgnoreCase);
            return true;
        }

        if (value is IDictionary<string, object> dictObj)
        {
            dictionary = dictObj.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value, StringComparer.OrdinalIgnoreCase);
            return true;
        }

        if (value is IDictionary legacy)
        {
            var map = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in legacy)
            {
                var key = entry.Key?.ToString();
                if (string.IsNullOrWhiteSpace(key))
                    continue;
                map[key] = entry.Value;
            }

            dictionary = map;
            return true;
        }

        dictionary = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        return false;
    }

    private static bool TryGetList(object value, out IReadOnlyList<object?> list)
    {
        if (value is IReadOnlyList<object?> readOnly)
        {
            list = readOnly;
            return true;
        }

        if (value is IEnumerable enumerable && value is not string)
        {
            var items = new List<object?>();
            foreach (var item in enumerable)
                items.Add(item);
            list = items;
            return true;
        }

        list = Array.Empty<object?>();
        return false;
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
        if (maxWidth <= 0)
            return text;

        if (text.Length <= maxWidth)
            return text;

        if (maxWidth <= 3)
            return text[..maxWidth];

        return text[..(maxWidth - 3)] + "...";
    }
}
