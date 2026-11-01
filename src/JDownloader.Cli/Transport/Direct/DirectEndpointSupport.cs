using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport.Direct;

/// <summary>
/// JDownloader's local API serves the same device namespaces as My.JDownloader, so almost every plan maps 1:1.
/// What does not exist locally is the My.JDownloader cloud layer itself; those endpoints are rejected up front
/// with an explanation instead of failing opaquely (or, worse, being sent anywhere else).
/// </summary>
internal static class DirectEndpointSupport
{
    private static readonly (string Prefix, string Reason)[] Unsupported =
    [
        ("/my/", "this is a My.JDownloader cloud endpoint, not a JDownloader device endpoint"),
        ("/session/", "the local API has no sessions (its handshake accepts anything and is not a real authentication)"),
        ("/device/getSessionPublicKey", "session encryption is part of the My.JDownloader relay and returns nothing locally"),
        ("/t_", "relay-routed device paths only exist on api.jdownloader.org"),
    ];

    public static void EnsureSupported(MyJdRequestPlan plan)
    {
        var endpoint = plan.Endpoint.Trim();
        foreach (var (prefix, reason) in Unsupported)
        {
            if (!endpoint.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;

            throw CliException.Usage(
                $"'{plan.Operation}' ({endpoint}) is not available over a direct connection: {reason}.",
                "Use a My.JDownloader connection for this operation (see 'jdr connection add <name> --mode myjd').");
        }
    }
}
