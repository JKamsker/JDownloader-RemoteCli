using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport;

public sealed class DeviceCatalog : IDeviceCatalog
{
    private readonly IMyJdRelayClient _relayClient;
    private readonly IProfileStore _profileStore;

    public DeviceCatalog(IMyJdRelayClient relayClient, IProfileStore profileStore)
    {
        _relayClient = relayClient;
        _profileStore = profileStore;
    }

    public async Task<IReadOnlyList<ResolvedDevice>> SyncAsync(
        string profileName,
        string? accountEmail,
        int timeoutSeconds,
        bool persist,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accountEmail))
            return [];

        var liveDevices = await _relayClient.ListDevicesAsync(profileName, accountEmail, timeoutSeconds, cancellationToken);
        var resolved = liveDevices
            .Where(device => !string.IsNullOrWhiteSpace(device.Id))
            .Select(device => new ResolvedDevice(device.Id, string.IsNullOrWhiteSpace(device.Name) ? device.Id : device.Name))
            .ToList();

        if (!persist)
            return resolved;

        var config = await _profileStore.LoadAsync(cancellationToken);
        if (!config.Profiles.TryGetValue(profileName, out var profile))
        {
            profile = new ProfileRecord();
            config.Profiles[profileName] = profile;
        }

        var existing = profile.KnownDevices.ToDictionary(device => device.Id, StringComparer.Ordinal);
        foreach (var device in resolved)
        {
            existing[device.Id] = new KnownDeviceRecord
            {
                Id = device.Id,
                Name = device.Name,
                SeenAtUtc = DateTimeOffset.UtcNow,
            };
        }

        profile.KnownDevices = existing.Values
            .OrderBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(device => device.Id, StringComparer.Ordinal)
            .ToList();

        if (resolved.Count == 1 && string.IsNullOrWhiteSpace(profile.DefaultDeviceId) && string.IsNullOrWhiteSpace(profile.DefaultDeviceName))
        {
            profile.DefaultDeviceId = resolved[0].Id;
            profile.DefaultDeviceName = resolved[0].Name;
        }
        else if (!string.IsNullOrWhiteSpace(profile.DefaultDeviceId)
            && existing.TryGetValue(profile.DefaultDeviceId, out var defaultDevice))
        {
            profile.DefaultDeviceName = defaultDevice.Name;
        }

        await _profileStore.SaveAsync(config, cancellationToken);
        return resolved;
    }
}
