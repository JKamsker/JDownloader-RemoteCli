using System.Collections;
using JDownloader.Cli.Commands.Connection;
using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport.Direct;

public sealed record DirectApiResult(object? Data);

/// <summary>Sends one call to JDownloader's local Remote API and returns the unwrapped payload.</summary>
public interface IDirectApiClient
{
    Task<DirectApiResult> InvokeAsync(
        DirectEndpoint endpoint,
        string path,
        IReadOnlyList<object?> parameters,
        int timeoutSeconds,
        CancellationToken cancellationToken);
}

/// <summary>
/// Executes request plans against JDownloader's built-in local HTTP API. It reuses the My.JDownloader
/// parameter mapping (same positional parameter lists) but never touches My.JDownloader or the network
/// beyond the configured endpoint.
/// </summary>
public sealed class DirectTransport : IMyJdTransport
{
    private readonly IDirectApiClient _client;

    public DirectTransport(IDirectApiClient client)
    {
        _client = client;
    }

    public async Task<MyJdTransportResult> ExecuteAsync(ResolvedProfileContext resolved, MyJdRequestPlan plan, CancellationToken cancellationToken)
    {
        if (!resolved.IsDirect || resolved.Direct is null)
            throw new InvalidOperationException("DirectTransport requires a direct connection profile.");

        DirectEndpointSupport.EnsureSupported(plan);

        var (parameters, mapperWarnings) = MyJdParameterPreparation.Prepare(plan);
        var result = await _client.InvokeAsync(
            resolved.Direct,
            plan.Endpoint,
            ToPositional(parameters),
            resolved.TimeoutSeconds,
            cancellationToken);

        var warnings = new List<string>();
        if (mapperWarnings is not null)
            warnings.AddRange(mapperWarnings);
        if (!resolved.Direct.IsLoopback)
            warnings.Add(ConnectionWarnings.RemoteHost(resolved.Direct.Host, resolved.Direct.Port, resolved.Direct.UseHttps));

        return new MyJdTransportResult(result.Data, warnings.Count == 0 ? null : warnings);
    }

    private static IReadOnlyList<object?> ToPositional(object? parameters)
    {
        return parameters switch
        {
            null => [],
            string or IDictionary => [parameters],
            IEnumerable sequence => sequence.Cast<object?>().ToList(),
            _ => [parameters],
        };
    }
}
