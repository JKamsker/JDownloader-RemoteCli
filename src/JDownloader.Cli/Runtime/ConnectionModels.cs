using System.Net;
using JDownloader.Cli.Config;

namespace JDownloader.Cli.Runtime;

public enum ConnectionMode
{
    MyJd,
    Direct,
}

public static class ConnectionModes
{
    public const string MyJdName = "myjd";
    public const string DirectName = "direct";

    public static string ToName(ConnectionMode mode) => mode == ConnectionMode.Direct ? DirectName : MyJdName;

    public static ConnectionMode Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ConnectionMode.MyJd;

        return value.Trim().ToLowerInvariant() switch
        {
            MyJdName or "my.jdownloader" or "cloud" => ConnectionMode.MyJd,
            DirectName or "local" => ConnectionMode.Direct,
            _ => throw CliException.Usage($"Unsupported connection mode '{value}'. Use '{MyJdName}' or '{DirectName}'."),
        };
    }
}

/// <summary>Where a direct-mode profile sends its requests (JDownloader's local HTTP Remote API).</summary>
public sealed record DirectEndpoint(string Host, int Port, bool UseHttps, bool AllowRemote)
{
    private static readonly System.Text.RegularExpressions.Regex HostNamePattern =
        new("^[A-Za-z0-9]([A-Za-z0-9._-]*[A-Za-z0-9])?$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public const string DefaultHost = "127.0.0.1";
    public const int DefaultPort = 3128;

    public bool IsLoopback => IsLoopbackHost(Host);

    public string Scheme => UseHttps ? "https" : "http";

    public Uri BaseUri => new UriBuilder(Scheme, Host.Trim('[', ']'), Port).Uri;

    public string DisplayValue => $"{Scheme}://{(Host.Contains(':') ? $"[{Host.Trim('[', ']')}]" : Host)}:{Port}";

    public static bool IsLoopbackHost(string host)
    {
        var trimmed = host.Trim().Trim('[', ']');
        // Only the exact name and IP literals count: other names (including *.localhost) depend on the resolver.
        if (string.Equals(trimmed, "localhost", StringComparison.OrdinalIgnoreCase))
            return true;

        return IPAddress.TryParse(trimmed, out var address) && IPAddress.IsLoopback(address);
    }

    public static void Validate(string? host, int? port)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw CliException.Usage("Direct connections require a host.");
        var bare = host.Trim().Trim('[', ']');
        if (!IPAddress.TryParse(bare, out _) && !HostNamePattern.IsMatch(bare))
            throw CliException.Usage($"Host '{host}' must be a bare hostname or IP address (no scheme, path, port or credentials). Use --port and --https for the rest.");
        if (port is < 1 or > 65535)
            throw CliException.Usage($"Port {port} is out of range (1-65535).");
    }

    public static DirectEndpoint FromProfile(string profileName, ProfileRecord profile)
    {
        var host = string.IsNullOrWhiteSpace(profile.Host) ? DefaultHost : profile.Host.Trim();
        var port = profile.Port ?? DefaultPort;
        Validate(host, port);

        var endpoint = new DirectEndpoint(host, port, profile.UseHttps ?? false, profile.AllowRemote ?? false);
        if (!endpoint.IsLoopback && !endpoint.AllowRemote)
        {
            throw CliException.Usage(
                $"Profile '{profileName}' targets non-loopback host '{host}' but remote access was not explicitly allowed.",
                $"Re-run 'jdr connection add {profileName} --mode direct --host {host} --port {port} --allow-remote' after reading the security notes in the README.");
        }

        return endpoint;
    }
}
