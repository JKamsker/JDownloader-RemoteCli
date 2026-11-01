using System.Net;
using System.Net.Sockets;
using System.Text;
using JDownloader.Cli.Transport;
using JDownloader.Cli.Transport.Direct;
using Microsoft.Extensions.DependencyInjection;

namespace JDownloader.Cli.Tests.Support;

internal sealed record CapturedRequest(HttpMethod Method, Uri Uri, string? ContentType, string Body, bool HasOrigin);

internal sealed class FakeDirectHandler : HttpMessageHandler
{
    private readonly Func<CapturedRequest, CancellationToken, Task<HttpResponseMessage>> _respond;

    public FakeDirectHandler(Func<CapturedRequest, CancellationToken, Task<HttpResponseMessage>> respond)
    {
        _respond = respond;
    }

    public List<CapturedRequest> Requests { get; } = [];

    public static FakeDirectHandler Json(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new((_, _) => Task.FromResult(JsonResponse(json, status)));

    public static FakeDirectHandler Throwing(Exception exception)
        => new((_, _) => throw exception);

    public static HttpRequestException ConnectionRefused()
        => new("refused", new SocketException((int)SocketError.ConnectionRefused));

    public static HttpResponseMessage JsonResponse(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        var captured = new CapturedRequest(
            request.Method,
            request.RequestUri!,
            request.Content?.Headers.ContentType?.MediaType,
            body,
            request.Headers.Contains("Origin"));
        Requests.Add(captured);
        return await _respond(captured, cancellationToken);
    }

    /// <summary>Service overrides: direct calls hit this handler, and any My.JDownloader relay call is recorded.</summary>
    public Action<IServiceCollection> Configure(RecordingRelayClient relay) => services =>
    {
        services.AddSingleton<IDirectHttpClientFactory>(new HandlerFactory(this));
        services.AddSingleton<IMyJdRelayClient>(relay);
    };

    private sealed class HandlerFactory : IDirectHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public HandlerFactory(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public HttpClient Create() => new(_handler, disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };
    }
}
