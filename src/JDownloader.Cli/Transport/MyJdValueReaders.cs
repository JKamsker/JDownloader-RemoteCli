namespace JDownloader.Cli.Transport;

internal static class MyJdValueReaders
{
    public static bool TryReadInt(object? value, out int number)
    {
        switch (value)
        {
            case int intValue:
                number = intValue;
                return true;
            case long longValue when longValue is >= int.MinValue and <= int.MaxValue:
                number = (int)longValue;
                return true;
            case string stringValue when int.TryParse(stringValue, out var parsed):
                number = parsed;
                return true;
            default:
                number = 0;
                return false;
        }
    }

    public static bool TryReadLong(object? value, out long number)
    {
        switch (value)
        {
            case long longValue:
                number = longValue;
                return true;
            case int intValue:
                number = intValue;
                return true;
            case string stringValue when long.TryParse(stringValue, out var parsed):
                number = parsed;
                return true;
            default:
                number = 0;
                return false;
        }
    }

    public static bool TryReadBool(object? value, out bool result)
    {
        switch (value)
        {
            case bool boolValue:
                result = boolValue;
                return true;
            case string stringValue when bool.TryParse(stringValue, out var parsed):
                result = parsed;
                return true;
            case int intValue when intValue is 0 or 1:
                result = intValue == 1;
                return true;
            case long longValue when longValue is 0 or 1:
                result = longValue == 1;
                return true;
            default:
                result = false;
                return false;
        }
    }

    public static bool TryReadLongArray(object? value, out long[] numbers)
    {
        var items = new List<long>();
        foreach (var entry in EnumerateValues(value))
        {
            if (entry is long longValue)
            {
                items.Add(longValue);
                continue;
            }

            if (entry is int intValue)
            {
                items.Add(intValue);
                continue;
            }

            if (entry is string stringValue && long.TryParse(stringValue, out var parsed))
            {
                items.Add(parsed);
                continue;
            }

            numbers = [];
            return false;
        }

        numbers = items.ToArray();
        return numbers.Length > 0;
    }

    public static IReadOnlyList<string> ToStringList(object? value)
    {
        return EnumerateValues(value)
            .Select(item => item?.ToString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .ToArray();
    }

    public static IEnumerable<object?> EnumerateValues(object? value)
    {
        return value switch
        {
            null => [],
            string => [value],
            IEnumerable<object?> objectValues => objectValues,
            Array array => array.Cast<object?>(),
            _ => [value],
        };
    }

    public static bool IsEmpty(object? value)
    {
        return value switch
        {
            null => true,
            string stringValue => string.IsNullOrWhiteSpace(stringValue),
            Dictionary<string, object?> dictionary => dictionary.Count == 0,
            IEnumerable<object?> items => !items.Any(),
            Array array => array.Length == 0,
            _ => false,
        };
    }
}
