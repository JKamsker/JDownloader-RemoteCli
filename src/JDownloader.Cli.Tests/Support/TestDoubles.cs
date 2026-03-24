using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;

namespace JDownloader.Cli.Tests.Support;

internal sealed class InMemoryProfileStore : IProfileStore
{
    public Jd2Config Config { get; set; } = new();

    public Task<Jd2Config> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(Config);

    public Task SaveAsync(Jd2Config config, CancellationToken cancellationToken)
    {
        Config = config;
        return Task.CompletedTask;
    }
}

internal sealed class RecordingRelayClient : IMyJdRelayClient
{
    public List<(ResolvedProfileContext Resolved, string Endpoint, object? Parameters)> Invocations { get; } = [];
    public IReadOnlyList<MyJdDeviceSummary> Devices { get; set; } = [];
    public object? Response { get; set; }

    public Task<IReadOnlyList<MyJdDeviceSummary>> ListDevicesAsync(string profileName, string? accountEmail, int timeoutSeconds, CancellationToken cancellationToken)
        => Task.FromResult(Devices);

    public Task<object?> InvokeAsync(ResolvedProfileContext resolved, string endpoint, object? parameters, CancellationToken cancellationToken)
    {
        Invocations.Add((resolved, endpoint, parameters));
        return Task.FromResult(Response);
    }
}

internal sealed class StubDeviceCatalog : IDeviceCatalog
{
    public required Func<string, string?, int, bool, CancellationToken, Task<IReadOnlyList<ResolvedDevice>>> SyncAsyncFunc { get; init; }

    public Task<IReadOnlyList<ResolvedDevice>> SyncAsync(
        string profileName,
        string? accountEmail,
        int timeoutSeconds,
        bool persist,
        CancellationToken cancellationToken)
        => SyncAsyncFunc(profileName, accountEmail, timeoutSeconds, persist, cancellationToken);
}

internal sealed class StubTransport : IMyJdTransport
{
    public required Func<ResolvedProfileContext, MyJdRequestPlan, CancellationToken, Task<MyJdTransportResult>> ExecuteAsyncFunc { get; init; }

    public Task<MyJdTransportResult> ExecuteAsync(ResolvedProfileContext resolved, MyJdRequestPlan plan, CancellationToken cancellationToken)
        => ExecuteAsyncFunc(resolved, plan, cancellationToken);
}
