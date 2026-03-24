using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport;

public sealed record MyJdDeviceSummary(string Id, string Name, string? Type, string? Status);

public interface IDeviceCatalog
{
    Task<IReadOnlyList<ResolvedDevice>> SyncAsync(
        string profileName,
        string? accountEmail,
        int timeoutSeconds,
        bool persist,
        CancellationToken cancellationToken);
}

public interface IMyJdRelayClient
{
    Task<IReadOnlyList<MyJdDeviceSummary>> ListDevicesAsync(string profileName, string? accountEmail, int timeoutSeconds, CancellationToken cancellationToken);
    Task<object?> InvokeAsync(ResolvedProfileContext resolved, string endpoint, object? parameters, CancellationToken cancellationToken);
}
