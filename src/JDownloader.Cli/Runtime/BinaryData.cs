namespace JDownloader.Cli.Runtime;

public static class BinaryData
{
    public static byte[] DecodeBytesOrThrow(object? data, string context)
    {
        if (data is null)
            throw CliException.Transport($"My.JDownloader returned no data for {context}.");

        if (data is byte[] bytes)
            return bytes;

        if (data is string text)
        {
            var trimmed = text.Trim();
            if (trimmed.Length == 0)
                throw CliException.Transport($"My.JDownloader returned an empty string for {context}.");

            try
            {
                return Convert.FromBase64String(trimmed);
            }
            catch (FormatException)
            {
                throw CliException.Transport($"My.JDownloader returned non-binary data for {context}.", trimmed.Length > 200 ? trimmed[..200] + "..." : trimmed);
            }
        }

        if (data is IEnumerable<object?> items)
        {
            var list = items.ToList();
            if (list.Count == 0)
                return [];

            var result = new byte[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (!TryConvertByte(item, out var value))
                    throw CliException.Transport($"My.JDownloader returned unexpected binary element for {context}.", item?.ToString() ?? "(null)");
                result[i] = value;
            }

            return result;
        }

        throw CliException.Transport($"My.JDownloader returned unexpected data for {context}.", data.ToString() ?? "(unknown)");
    }

    public static void WriteAllBytes(string outputFile, byte[] bytes)
    {
        if (string.IsNullOrWhiteSpace(outputFile))
            throw CliException.Usage("Binary output requires --output-file <path>.");

        var fullPath = Path.GetFullPath(outputFile.Trim());
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllBytes(fullPath, bytes);
    }

    private static bool TryConvertByte(object? value, out byte result)
    {
        switch (value)
        {
            case byte b:
                result = b;
                return true;
            case sbyte sb when sb >= 0:
                result = (byte)sb;
                return true;
            case short s when s is >= 0 and <= 255:
                result = (byte)s;
                return true;
            case int i when i is >= 0 and <= 255:
                result = (byte)i;
                return true;
            case long l when l is >= 0 and <= 255:
                result = (byte)l;
                return true;
            case double d when d is >= 0 and <= 255 && Math.Abs(d % 1) < double.Epsilon:
                result = (byte)d;
                return true;
            case float f when f is >= 0 and <= 255 && Math.Abs(f % 1) < float.Epsilon:
                result = (byte)f;
                return true;
            case string text when byte.TryParse(text, out var parsed):
                result = parsed;
                return true;
            default:
                result = 0;
                return false;
        }
    }
}

