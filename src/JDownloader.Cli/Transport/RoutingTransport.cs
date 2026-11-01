using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport.Direct;

namespace JDownloader.Cli.Transport;

/// <summary>
/// Chooses the transport from the resolved connection mode. There is deliberately no fallback:
/// a direct profile only ever reaches <see cref="DirectTransport"/>, a My.JDownloader profile only <see cref="LiveMyJdTransport"/>.
/// </summary>
public sealed class RoutingTransport : IMyJdTransport
{
    private readonly LiveMyJdTransport _myJd;
    private readonly DirectTransport _direct;

    public RoutingTransport(LiveMyJdTransport myJd, DirectTransport direct)
    {
        _myJd = myJd;
        _direct = direct;
    }

    public Task<MyJdTransportResult> ExecuteAsync(ResolvedProfileContext resolved, MyJdRequestPlan plan, CancellationToken cancellationToken)
    {
        return resolved.ConnectionMode switch
        {
            ConnectionMode.Direct => _direct.ExecuteAsync(resolved, plan, cancellationToken),
            ConnectionMode.MyJd => _myJd.ExecuteAsync(resolved, plan, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported connection mode '{resolved.ConnectionMode}'."),
        };
    }
}
