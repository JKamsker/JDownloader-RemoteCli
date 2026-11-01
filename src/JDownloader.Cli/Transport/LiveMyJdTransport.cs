using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport;

public sealed class LiveMyJdTransport : IMyJdTransport
{
    private readonly IMyJdRelayClient _relayClient;

    public LiveMyJdTransport(IMyJdRelayClient relayClient)
    {
        _relayClient = relayClient;
    }

    public async Task<MyJdTransportResult> ExecuteAsync(ResolvedProfileContext resolved, MyJdRequestPlan plan, CancellationToken cancellationToken)
    {
        if (resolved.IsDirect)
            throw new InvalidOperationException("A direct connection must never be routed through My.JDownloader.");

        var (parameters, warnings) = MyJdParameterPreparation.Prepare(plan);
        var data = await _relayClient.InvokeAsync(resolved, plan.Endpoint, parameters, cancellationToken);
        return new MyJdTransportResult(data, warnings);
    }
}
