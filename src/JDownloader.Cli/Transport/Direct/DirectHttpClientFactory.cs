
namespace JDownloader.Cli.Transport.Direct;

/// <summary>Creates the HttpClient used for direct (local API) calls. Replaced in tests with a mocked handler.</summary>
public interface IDirectHttpClientFactory
{
    HttpClient Create();
}

public sealed class DirectHttpClientFactory : IDirectHttpClientFactory
{
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(5);

    public HttpClient Create()
    {
        // Direct mode must never route through a system proxy: it talks to one specific JDownloader instance.
        var handler = new SocketsHttpHandler
        {
            ConnectTimeout = ConnectTimeout,
            UseProxy = false,
            AllowAutoRedirect = false,
        };

        // The overall timeout is enforced per call with a linked CancellationTokenSource.
        return new HttpClient(handler, disposeHandler: true) { Timeout = Timeout.InfiniteTimeSpan };
    }
}
