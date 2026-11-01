namespace JDownloader.Cli.Commands.Connection;

public static class ConnectionWarnings
{
    public static string RemoteHost(string host, int port, bool https)
    {
        var scheme = https ? "https" : "http";
        return $"Security: {scheme}://{host}:{port} is not a loopback address. JDownloader's local API has no authentication; "
            + (https ? "anyone who can reach it can control JDownloader." : "anyone who can reach it can control JDownloader, and traffic is unencrypted.")
            + " Prefer an SSH tunnel to 127.0.0.1 and never expose the port to the internet.";
    }
}
