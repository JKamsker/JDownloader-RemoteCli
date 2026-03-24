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

        if (HumanDataShape.TryGetDictionary(data, out var dictionary))
            return RenderObject(dictionary);

        if (HumanDataShape.TryGetList(data, out var list))
            return RenderList(list);

        return [HumanValueFormatter.ToDisplayString(data, keyHint: null, maxWidth: 0)];
    }

    private static IReadOnlyList<string> RenderObject(IReadOnlyDictionary<string, object?> dictionary)
    {
        if (dictionary.Count == 0)
            return ["(empty)"];

        var lines = new List<string>(dictionary.Count);
        foreach (var (key, value) in dictionary.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
            lines.Add($"{key}: {HumanValueFormatter.ToDisplayString(value, key, maxWidth: 200)}");

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
            if (!HumanDataShape.TryGetDictionary(item, out var dictionary))
            {
                dictionaries.Clear();
                break;
            }

            dictionaries.Add(dictionary);
        }

        if (dictionaries.Count > 0)
            return HumanTableRenderer.RenderTable(dictionaries, PreferredColumns);

        var lines = new List<string>(items.Count);
        foreach (var item in items)
            lines.Add(HumanValueFormatter.ToDisplayString(item, keyHint: null, maxWidth: 0));
        return lines;
    }
}
