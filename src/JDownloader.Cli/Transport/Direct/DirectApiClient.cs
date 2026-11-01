using System.Net.Sockets;
using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport.Direct;

public sealed class DirectApiClient : IDirectApiClient
{
    private readonly Lazy<HttpClient> _httpClient;

    public DirectApiClient(IDirectHttpClientFactory httpClientFactory)
    {
        _httpClient = new Lazy<HttpClient>(httpClientFactory.Create);
    }

    public async Task<DirectApiResult> InvokeAsync(
        DirectEndpoint endpoint,
        string path,
        IReadOnlyList<object?> parameters,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds)));

        try
        {
            using var request = DirectRequestEncoder.Create(endpoint.BaseUri, path, parameters);
            using var response = await _httpClient.Value.SendAsync(request, HttpCompletionOption.ResponseContentRead, timeoutCts.Token);
            var body = await response.Content.ReadAsByteArrayAsync(timeoutCts.Token);
            var data = DirectResponseReader.Read(response.StatusCode, response.Content.Headers.ContentType?.MediaType, body, endpoint, path);
            return new DirectApiResult(data);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeoutCts.IsCancellationRequested)
        {
            throw CliException.Transport(
                $"Timed out after {timeoutSeconds}s waiting for JDownloader at {endpoint.DisplayValue}.",
                "Check that JDownloader is running and responsive, or raise --timeout.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // SocketsHttpHandler.ConnectTimeout surfaces as a cancellation that is neither ours nor the caller's.
            throw CliException.Transport(
                $"Connecting to {endpoint.DisplayValue} timed out.",
                "Check network reachability and any firewall between this machine and JDownloader.");
        }
        catch (HttpRequestException ex)
        {
            throw MapConnectionFailure(endpoint, ex);
        }
    }

    internal static CliException MapConnectionFailure(DirectEndpoint endpoint, HttpRequestException exception)
    {
        if (exception.InnerException is SocketException socket)
        {
            return socket.SocketErrorCode switch
            {
                SocketError.ConnectionRefused => CliException.Transport(
                    $"Connection refused by {endpoint.DisplayValue}. JDownloader is not listening there.",
                    "Start JDownloader and enable the local API (Advanced Settings: RemoteAPIConfig 'deprecatedapienabled'), or check --host/--port ('jdr connection get <name>')."),
                SocketError.HostNotFound or SocketError.NoData or SocketError.TryAgain => CliException.Transport(
                    $"Host '{endpoint.Host}' could not be resolved.",
                    "Check the host name in 'jdr connection get <name>'."),
                SocketError.TimedOut => CliException.Transport(
                    $"Connecting to {endpoint.DisplayValue} timed out.",
                    "Check network reachability and any firewall between this machine and JDownloader."),
                SocketError.NetworkUnreachable or SocketError.HostUnreachable => CliException.Transport(
                    $"{endpoint.DisplayValue} is unreachable from this machine.",
                    "Check network connectivity and the configured host."),
                _ => CliException.Transport($"Could not connect to {endpoint.DisplayValue}: {socket.Message}"),
            };
        }

        return CliException.Transport($"Direct call to {endpoint.DisplayValue} failed: {exception.Message}");
    }
}
