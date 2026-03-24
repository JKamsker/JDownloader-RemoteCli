using System.Collections;

namespace JDownloader.Cli.Runtime;

internal static class HumanDataShape
{
    public static bool TryGetDictionary(object value, out IReadOnlyDictionary<string, object?> dictionary)
    {
        if (value is IReadOnlyDictionary<string, object?> readOnly)
        {
            dictionary = new Dictionary<string, object?>(readOnly, StringComparer.OrdinalIgnoreCase);
            return true;
        }

        if (value is IDictionary<string, object?> dictionaryValue)
        {
            dictionary = new Dictionary<string, object?>(dictionaryValue, StringComparer.OrdinalIgnoreCase);
            return true;
        }

        if (value is IDictionary<string, object> objectDictionary)
        {
            dictionary = objectDictionary.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value, StringComparer.OrdinalIgnoreCase);
            return true;
        }

        if (value is IDictionary legacy)
        {
            var map = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in legacy)
            {
                var key = entry.Key?.ToString();
                if (!string.IsNullOrWhiteSpace(key))
                    map[key] = entry.Value;
            }

            dictionary = map;
            return true;
        }

        dictionary = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        return false;
    }

    public static bool TryGetList(object value, out IReadOnlyList<object?> list)
    {
        if (value is IReadOnlyList<object?> readOnly)
        {
            list = readOnly;
            return true;
        }

        if (value is IEnumerable enumerable and not string)
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
}
