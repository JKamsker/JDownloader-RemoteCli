using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JDownloader.Cli.Transport.Direct;

/// <summary>
/// Encodes a positional parameter list for JDownloader's local API: <c>POST /namespace/method</c> with
/// <c>application/json</c> and a <c>{"params":[...]}</c> body (verified against a live JDownloader: a bare
/// array is rejected with HTTP 500, and JSON-encoded strings for object parameters with HTTP 400).
/// </summary>
/// <remarks>
/// My.JDownloader passes object parameters as JSON-encoded strings (<c>"{\"maxResults\":10}"</c>), which its
/// device layer unwraps. The local server does not: it needs real JSON values, so the strings the mapper
/// generated for query objects (see <see cref="JsonStringParameterMarker"/>) are expanded here; every other
/// string is sent as a plain JSON string. Neither <c>rid</c>, <c>url</c> nor <c>ApiVer</c> is needed locally, and no
/// <c>Origin</c> header is ever sent because the server rejects unknown origins.
/// </remarks>
internal static class DirectRequestEncoder
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = null };

    public static HttpRequestMessage Create(Uri baseUri, string path, IReadOnlyList<object?> parameters)
    {
        var normalizedPath = path.StartsWith('/') ? path : "/" + path;
        var target = new Uri(baseUri, normalizedPath);
        if (!string.Equals(target.Scheme, baseUri.Scheme, StringComparison.Ordinal)
            || !string.Equals(target.Authority, baseUri.Authority, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Refusing to send a direct request outside {baseUri.Authority}: '{path}'.");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, target)
        {
            Content = new StringContent(BuildBody(parameters), Encoding.UTF8, "application/json"),
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    public static string BuildBody(IReadOnlyList<object?> parameters)
    {
        var array = new JsonArray();
        foreach (var parameter in parameters)
            array.Add(ToNode(parameter));

        return new JsonObject { ["params"] = array }.ToJsonString(JsonOptions);
    }

    private static JsonNode? ToNode(object? value)
    {
        if (value is string text && JsonStringParameterMarker.IsMarked(text))
            return JsonNode.Parse(text);

        return value is null ? null : JsonSerializer.SerializeToNode(value, JsonOptions);
    }
}
