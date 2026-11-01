using System.Net;
using System.Text;
using System.Text.Json;
using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport.Direct;

/// <summary>
/// Interprets responses of JDownloader's local API: success is <c>{"data": …, "rid": -1}</c>, errors are
/// <c>{"src":"DEVICE","type":"…","data":…}</c> with a matching HTTP status. Raw endpoints (icons) return bytes.
/// </summary>
internal static class DirectResponseReader
{
    public static object? Read(HttpStatusCode status, string? mediaType, byte[] body, DirectEndpoint endpoint, string path)
    {
        var isJson = mediaType is not null && mediaType.Contains("json", StringComparison.OrdinalIgnoreCase);
        if ((int)status is >= 200 and < 300)
            return ReadSuccess(isJson, mediaType, body, endpoint, path);

        throw MapError(status, isJson, body, endpoint, path);
    }

    private static object? ReadSuccess(bool isJson, string? mediaType, byte[] body, DirectEndpoint endpoint, string path)
    {
        if (isJson)
        {
            try
            {
                using var document = JsonDocument.Parse(body);
                return Unwrap(document.RootElement);
            }
            catch (JsonException ex)
            {
                throw CliException.Transport($"{endpoint.DisplayValue} returned malformed JSON for {path}: {ex.Message}");
            }
        }

        if (mediaType is not null && !mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
            return body;

        throw CliException.Transport(
            $"{endpoint.DisplayValue} answered {path} with a non-JSON response ({mediaType ?? "no content type"}): {Preview(body)}",
            "This does not look like JDownloader's local API. Check --host/--port ('jdr connection get <name>').");
    }

    private static object? Unwrap(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (string.Equals(property.Name, "data", StringComparison.OrdinalIgnoreCase))
                    return JsonInput.ConvertElement(property.Value);
            }
        }

        return JsonInput.ConvertElement(root);
    }

    private static CliException MapError(HttpStatusCode status, bool isJson, byte[] body, DirectEndpoint endpoint, string path)
    {
        var code = (int)status;
        var errorType = TryReadErrorType(isJson, body);
        var detail = errorType is null ? Preview(body) : $"{errorType}: {Preview(body)}";

        return code switch
        {
            400 => CliException.Transport(
                $"JDownloader rejected the parameters for {path} (HTTP 400{Suffix(errorType)}).",
                "The request shape does not match this JDownloader version. Re-run with --verbose or report the endpoint."),
            403 => CliException.Transport(
                $"JDownloader refused the request to {path} (HTTP 403{Suffix(errorType)}): {detail}",
                "If you reach JDownloader through a browser-style proxy, make sure no Origin header is added."),
            404 => CliException.Transport(
                $"JDownloader at {endpoint.DisplayValue} has no endpoint {path} (HTTP 404{Suffix(errorType)}).",
                "This JDownloader build may not support the operation. If every call returns 404, another service may be listening on this port."),
            >= 500 => CliException.Transport(
                $"JDownloader reported an internal error for {path} (HTTP {code}{Suffix(errorType)}): {detail}"),
            _ => CliException.Transport($"Direct call {path} failed with HTTP {code}: {detail}"),
        };
    }

    private static string Suffix(string? errorType) => errorType is null ? string.Empty : $", {errorType}";

    private static string? TryReadErrorType(bool isJson, byte[] body)
    {
        if (!isJson)
            return null;

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("type", out var type)
                && type.ValueKind == JsonValueKind.String)
            {
                return type.GetString();
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }

    private static string Preview(byte[] body)
    {
        var text = Encoding.UTF8.GetString(body).Trim();
        if (text.Length == 0)
            return "(empty response)";
        return text.Length > 200 ? text[..200] + "..." : text;
    }
}
