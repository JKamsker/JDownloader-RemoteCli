using System.Text.Json;
using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport;

public sealed record MyJdRequestPlan(
    string Operation,
    string Method,
    string Endpoint,
    object? Query,
    object? Body,
    bool Destructive,
    bool ProducesBinary,
    string? DeviceId = null,
    bool PreserveRawParameters = false);

public sealed record MyJdTransportResult(object? Data, IReadOnlyList<string>? Warnings = null);

public interface IRequestIdProvider
{
    long Next();
}

public interface IMyJdTransport
{
    Task<MyJdTransportResult> ExecuteAsync(ResolvedProfileContext resolved, MyJdRequestPlan plan, CancellationToken cancellationToken);
}

public sealed class TimestampRequestIdProvider : IRequestIdProvider
{
    private long _last = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public long Next() => Interlocked.Increment(ref _last);
}

public static class JsonInput
{
    public static object? ParseOptional(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var content = raw.Trim();
        if (content.StartsWith('@'))
        {
            var path = content[1..].Trim();
            if (string.IsNullOrWhiteSpace(path))
                throw CliException.Usage("JSON file input requires a path after '@', for example '@request.json'.");

            try
            {
                content = File.ReadAllText(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                throw CliException.Usage($"Could not read JSON from file '{path}': {ex.Message}");
            }
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            return ConvertElement(document.RootElement);
        }
        catch (JsonException ex)
        {
            throw CliException.Usage($"Invalid JSON input: {ex.Message}");
        }
    }

    private static object? ConvertElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                property => property.Name,
                property => ConvertElement(property.Value)),
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertElement).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var integer) ? integer : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText(),
        };
    }
}
