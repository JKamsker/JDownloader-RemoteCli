using System.Runtime.CompilerServices;

namespace JDownloader.Cli.Transport;

/// <summary>
/// My.JDownloader expects query objects as JSON-encoded strings; JDownloader's local API expects real JSON.
/// The mapper tags exactly those generated strings (by reference) so the direct transport can expand them
/// without touching user-supplied strings that merely look like JSON (passwords, config values, ...).
/// </summary>
internal static class JsonStringParameterMarker
{
    private static readonly ConditionalWeakTable<string, object> Marked = new();

    public static string Mark(string json)
    {
        Marked.TryAdd(json, new object());
        return json;
    }

    public static bool IsMarked(string value) => Marked.TryGetValue(value, out _);
}
