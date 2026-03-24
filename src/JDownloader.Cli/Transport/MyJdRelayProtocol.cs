using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport;

internal static class MyJdRelayProtocol
{
    private const string ApiBaseUrl = "https://api.jdownloader.org";
    internal const string AppKey = "jd2-cli";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static async Task<JsonDocument> SendServerGetAsync(
        HttpClient httpClient,
        IRequestIdProvider requestIdProvider,
        string relativePath,
        byte[] signingKey,
        byte[] responseKey,
        CancellationToken cancellationToken)
    {
        var rid = requestIdProvider.Next();
        var pathWithRid = relativePath.Contains('?')
            ? $"{relativePath}&rid={rid}"
            : $"{relativePath}?rid={rid}";
        var signature = ComputeSignature(pathWithRid, signingKey);
        var uri = new Uri($"{ApiBaseUrl}{pathWithRid}&signature={signature}");

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response.StatusCode, content, "My.JDownloader server call");

        var plaintext = DecryptBase64(content, responseKey);
        return JsonDocument.Parse(plaintext);
    }

    public static async Task<JsonDocument> SendDeviceActionAsync(
        HttpClient httpClient,
        IRequestIdProvider requestIdProvider,
        MyJdSession session,
        string deviceId,
        string endpoint,
        object? parameters,
        CancellationToken cancellationToken)
    {
        var action = new MyJdActionEnvelope
        {
            ApiVer = 1,
            Params = parameters,
            RequestId = requestIdProvider.Next(),
            Url = endpoint,
        };

        var uri = new Uri($"{ApiBaseUrl}/t_{Uri.EscapeDataString(session.SessionToken)}_{Uri.EscapeDataString(deviceId)}{endpoint}");
        var plaintext = JsonSerializer.Serialize(action, JsonOptions);
        var ciphertext = EncryptBase64(plaintext, session.DeviceEncryptionToken);

        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(ciphertext, Encoding.UTF8, "application/aesjson-jd"),
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/aesjson-jd");

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response.StatusCode, content, $"My.JDownloader device call '{endpoint}'");

        var decrypted = DecryptBase64(content, session.DeviceEncryptionToken);
        return JsonDocument.Parse(decrypted);
    }

    public static object? ExtractDataOrWhole(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object && TryGetProperty(element, "data", out var dataElement))
            return ConvertElement(dataElement);

        return ConvertElement(element);
    }

    public static byte[] UpdateEncryptionToken(byte[] secret, string sessionTokenHex)
    {
        var sessionBytes = HexToBytes(sessionTokenHex);
        var combined = new byte[secret.Length + sessionBytes.Length];
        Buffer.BlockCopy(secret, 0, combined, 0, secret.Length);
        Buffer.BlockCopy(sessionBytes, 0, combined, secret.Length, sessionBytes.Length);
        return SHA256.HashData(combined);
    }

    public static string GetRequiredString(JsonElement element, string propertyName)
    {
        var value = GetString(element, propertyName);
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        throw CliException.Transport($"My.JDownloader response did not include '{propertyName}'.");
    }

    public static string? GetString(JsonElement element, string propertyName)
    {
        return TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    public static byte[] HexToBytes(string hex)
    {
        return Convert.FromHexString(hex.Replace("-", string.Empty, StringComparison.Ordinal));
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

    private static void EnsureSuccess(HttpStatusCode statusCode, string body, string operation)
    {
        if ((int)statusCode is >= 200 and < 300)
            return;

        var preview = string.IsNullOrWhiteSpace(body) ? "(empty response)" : body.Trim();
        if (preview.Length > 200)
            preview = preview[..200] + "...";

        throw CliException.Transport($"{operation} failed with HTTP {(int)statusCode}.", preview);
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement property)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            property = default;
            return false;
        }

        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }

    private static string ComputeSignature(string data, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
    }

    private static string EncryptBase64(string plaintext, byte[] keyMaterial)
    {
        using var aes = CreateAes(keyMaterial);
        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
        return Convert.ToBase64String(ciphertext);
    }

    private static string DecryptBase64(string ciphertextBase64, byte[] keyMaterial)
    {
        using var aes = CreateAes(keyMaterial);
        using var decryptor = aes.CreateDecryptor();
        var ciphertext = Convert.FromBase64String(ciphertextBase64);
        var plaintextBytes = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
        return Encoding.UTF8.GetString(plaintextBytes);
    }

    private static Aes CreateAes(byte[] keyMaterial)
    {
        if (keyMaterial.Length < 32)
            throw new CryptographicException("My.JDownloader key material was shorter than expected.");

        var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.IV = keyMaterial[..16];
        aes.Key = keyMaterial[16..32];
        return aes;
    }
}

internal sealed record StoredRelayAuth(string Email, byte[] ServerSecret, byte[] DeviceSecret);
internal sealed record MyJdSession(string SessionToken, byte[] ServerEncryptionToken, byte[] DeviceEncryptionToken);

internal sealed class MyJdActionEnvelope
{
    [JsonPropertyName("ApiVer")]
    public int ApiVer { get; set; }

    [JsonPropertyName("params")]
    public object? Params { get; set; }

    [JsonPropertyName("rid")]
    public long RequestId { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; }
}
