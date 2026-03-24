namespace JDownloader.Cli.Runtime;

internal static class HumanTableRenderer
{
    public static IReadOnlyList<string> RenderTable(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        IReadOnlyList<string> preferredColumns)
    {
        if (rows.Count == 0)
            return ["(empty)"];

        var availableColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < Math.Min(rows.Count, 25); index++)
        {
            foreach (var key in rows[index].Keys)
                availableColumns.Add(key);
        }

        var columns = PickColumns(availableColumns, preferredColumns);
        if (columns.Count == 0)
            return ["(empty)"];

        var maxCellWidthByColumn = columns.ToDictionary(
            column => column,
            column => column.Equals("url", StringComparison.OrdinalIgnoreCase) ? 90 : 50,
            StringComparer.OrdinalIgnoreCase);

        var widths = columns.ToDictionary(column => column, column => column.Length, StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            foreach (var column in columns)
            {
                row.TryGetValue(column, out var value);
                var display = HumanValueFormatter.ToDisplayString(value, column, maxCellWidthByColumn[column]);
                widths[column] = Math.Min(Math.Max(widths[column], display.Length), maxCellWidthByColumn[column]);
            }
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
                var display = HumanValueFormatter.ToDisplayString(value, column, maxCellWidthByColumn[column]);
                cells.Add(Pad(display, widths[column]));
            }

            lines.Add(string.Join("  ", cells));
        }

        return lines;
    }

    private static List<string> PickColumns(HashSet<string> available, IReadOnlyList<string> preferredColumns)
    {
        const int maxColumns = 8;
        var columns = new List<string>(maxColumns);

        foreach (var preferred in preferredColumns)
        {
            if (available.Contains(preferred))
                columns.Add(preferred);
            if (columns.Count >= maxColumns)
                return columns;
        }

        var fallback = available
            .Where(key => preferredColumns.All(preferred => !preferred.Equals(key, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
            .Take(maxColumns - columns.Count);
        columns.AddRange(fallback);
        return columns;
    }

    private static string Pad(string value, int width)
    {
        if (value.Length >= width)
            return value;

        return value + new string(' ', width - value.Length);
    }
}
