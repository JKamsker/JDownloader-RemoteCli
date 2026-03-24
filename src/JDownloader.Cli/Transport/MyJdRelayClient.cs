using System.Security.Cryptography;
using System.Text.Json;
using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport;

public sealed class MyJdRelayClient : IMyJdRelayClient
{
    private readonly HttpClient _httpClient = new() { Timeout = Timeout.InfiniteTimeSpan };
    private readonly IProfileStore _profileStore;
    private readonly ICredentialProtector _protector;
    private readonly IRequestIdProvider _requestIdProvider;

    public MyJdRelayClient(
        IProfileStore profileStore,
        ICredentialProtector protector,
        IRequestIdProvider requestIdProvider)
    {
        _profileStore = profileStore;
        _protector = protector;
        _requestIdProvider = requestIdProvider;
    }

    public async Task<IReadOnlyList<MyJdDeviceSummary>> ListDevicesAsync(
        string profileName,
        string? accountEmail,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = CreateTimeoutSource(timeoutSeconds, cancellationToken);
        try
        {
            var auth = await LoadAuthAsync(profileName, accountEmail, timeoutCts.Token);
            var session = await ConnectAsync(auth, timeoutCts.Token);
            using var document = await MyJdRelayProtocol.SendServerGetAsync(
                _httpClient,
                _requestIdProvider,
                $"/my/listdevices?sessiontoken={Uri.EscapeDataString(session.SessionToken)}",
                session.ServerEncryptionToken,
                session.ServerEncryptionToken,
                timeoutCts.Token);

            if (!TryGetArrayProperty(document.RootElement, "list", out var listElement))
                return [];

            var devices = new List<MyJdDeviceSummary>();
            foreach (var item in listElement.EnumerateArray())
            {
                var id = MyJdRelayProtocol.GetString(item, "id");
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                devices.Add(new MyJdDeviceSummary(
                    id,
                    MyJdRelayProtocol.GetString(item, "name") ?? id,
                    MyJdRelayProtocol.GetString(item, "type"),
                    MyJdRelayProtocol.GetString(item, "status")));
            }

            return devices;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeoutCts.IsCancellationRequested)
        {
            throw CliException.Transport($"Timed out after {timeoutSeconds}s contacting My.JDownloader.");
        }
        catch (CliException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or CryptographicException or JsonException or FormatException or InvalidOperationException)
        {
            throw CliException.Transport($"My.JDownloader device discovery failed: {ex.Message}");
        }
    }

    public async Task<object?> InvokeAsync(ResolvedProfileContext resolved, string endpoint, object? parameters, CancellationToken cancellationToken)
    {
        if (resolved.Device is null)
            throw CliException.Usage("Device is required because no default device could be resolved.");

        using var timeoutCts = CreateTimeoutSource(resolved.TimeoutSeconds, cancellationToken);
        try
        {
            var auth = await LoadAuthAsync(resolved.ProfileName, resolved.AccountEmail, timeoutCts.Token);
            var session = await ConnectAsync(auth, timeoutCts.Token);
            using var document = await MyJdRelayProtocol.SendDeviceActionAsync(
                _httpClient,
                _requestIdProvider,
                session,
                resolved.Device.Id,
                endpoint,
                parameters,
                timeoutCts.Token);
            return MyJdRelayProtocol.ExtractDataOrWhole(document.RootElement);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeoutCts.IsCancellationRequested)
        {
            throw CliException.Transport($"Timed out after {resolved.TimeoutSeconds}s contacting My.JDownloader.");
        }
        catch (CliException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or CryptographicException or JsonException or FormatException or InvalidOperationException)
        {
            throw CliException.Transport($"My.JDownloader relay call failed: {ex.Message}");
        }
    }

    private async Task<StoredRelayAuth> LoadAuthAsync(string profileName, string? accountEmail, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accountEmail))
        {
            throw CliException.NotAuthenticated(
                "Authentication required for protected commands.",
                $"Run 'jdr auth login --profile {profileName} --email <email> --password-stdin'.");
        }

        var normalizedEmail = accountEmail.Trim().ToLowerInvariant();
        var config = await _profileStore.LoadAsync(cancellationToken);
        if (!config.Credentials.TryGetValue(normalizedEmail, out var credential) || credential.AuthBlob is null)
        {
            throw CliException.NotAuthenticated(
                "Authentication required for protected commands.",
                $"Run 'jdr auth login --profile {profileName} --email {normalizedEmail} --password-stdin'.");
        }

        StoredAuthMaterial? authMaterial;
        try
        {
            authMaterial = await _protector.UnprotectAsync<StoredAuthMaterial>(credential.AuthBlob, cancellationToken);
        }
        catch (CryptographicException)
        {
            authMaterial = null;
        }
        catch (FormatException)
        {
            authMaterial = null;
        }

        if (authMaterial is null)
        {
            throw CliException.NotAuthenticated(
                "Stored auth material could not be decrypted.",
                $"Run 'jdr auth login --profile {profileName} --email {normalizedEmail} --password-stdin'.");
        }

        if (string.IsNullOrWhiteSpace(authMaterial.ServerSecretHex) || string.IsNullOrWhiteSpace(authMaterial.DeviceSecretHex))
        {
            throw CliException.NotAuthenticated(
                "Saved auth material is from the initial scaffold and cannot authenticate live relay calls.",
                $"Run 'jdr auth login --profile {profileName} --email {normalizedEmail} --password-stdin' once to refresh it.");
        }

        return new StoredRelayAuth(
            normalizedEmail,
            MyJdRelayProtocol.HexToBytes(authMaterial.ServerSecretHex),
            MyJdRelayProtocol.HexToBytes(authMaterial.DeviceSecretHex));
    }

    private async Task<MyJdSession> ConnectAsync(StoredRelayAuth auth, CancellationToken cancellationToken)
    {
        using var document = await MyJdRelayProtocol.SendServerGetAsync(
            _httpClient,
            _requestIdProvider,
            $"/my/connect?email={Uri.EscapeDataString(auth.Email)}&appkey={Uri.EscapeDataString(MyJdRelayProtocol.AppKey)}",
            auth.ServerSecret,
            auth.ServerSecret,
            cancellationToken);

        var sessionToken = MyJdRelayProtocol.GetRequiredString(document.RootElement, "sessiontoken");
        var serverEncryptionToken = MyJdRelayProtocol.UpdateEncryptionToken(auth.ServerSecret, sessionToken);
        var deviceEncryptionToken = MyJdRelayProtocol.UpdateEncryptionToken(auth.DeviceSecret, sessionToken);
        return new MyJdSession(sessionToken, serverEncryptionToken, deviceEncryptionToken);
    }

    private static CancellationTokenSource CreateTimeoutSource(int timeoutSeconds, CancellationToken cancellationToken)
    {
        var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds)));
        return timeoutCts;
    }

    private static bool TryGetArrayProperty(JsonElement element, string propertyName, out JsonElement property)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            property = default;
            return false;
        }

        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase)
                && candidate.Value.ValueKind == JsonValueKind.Array)
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }
}
