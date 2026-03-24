using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Device;

public sealed class ListDevicesCommand : ProfileApiCommand<NoArgSettings>
{
    private readonly IProfileStore _profileStore;
    private readonly IDeviceCatalog _deviceCatalog;

    public ListDevicesCommand(
        IProfileResolver profileResolver,
        IDeviceCatalog deviceCatalog,
        IProfileStore profileStore,
        IOutputRenderer outputRenderer,
        IDiagnosticLogger diagnosticLogger)
        : base(profileResolver, outputRenderer, diagnosticLogger)
    {
        _deviceCatalog = deviceCatalog;
        _profileStore = profileStore;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(
        CommandContext context,
        NoArgSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        var warnings = new List<string>();
        var config = await _profileStore.LoadAsync(cancellationToken);
        IReadOnlyList<ResolvedDevice>? liveDevices = null;

        ProfileRecord profileRecord;
        if (!config.Profiles.TryGetValue(resolved.ProfileName, out var profile) || profile is null)
        {
            profileRecord = new ProfileRecord();
        }
        else
        {
            profileRecord = profile;
        }

        var hasKnownDevices = profileRecord.KnownDevices.Count > 0;

        if (!string.IsNullOrWhiteSpace(profileRecord.AccountEmail))
        {
            try
            {
                liveDevices = await _deviceCatalog.SyncAsync(
                    resolved.ProfileName,
                    profileRecord.AccountEmail,
                    resolved.TimeoutSeconds,
                    persist: !settings.DryRun,
                    cancellationToken);

                if (!settings.DryRun)
                {
                    config = await _profileStore.LoadAsync(cancellationToken);
                    if (!config.Profiles.TryGetValue(resolved.ProfileName, out profile) || profile is null)
                        profileRecord = new ProfileRecord();
                    else
                        profileRecord = profile;
                }
                else
                {
                    warnings.Add("Dry-run: device cache was not updated.");
                }
            }
            catch (CliException ex) when ((ex.Kind == "not_authenticated" || ex.Kind == "transport") && hasKnownDevices)
            {
                warnings.Add(ex.Message);
            }
        }

        var devices = (liveDevices?.Count > 0
            ? liveDevices.Select(device => new KnownDeviceRecord
            {
                Id = device.Id,
                Name = device.Name,
                SeenAtUtc = DateTimeOffset.UtcNow,
            })
            : profileRecord.KnownDevices).ToList();

        var items = devices.Select(device => new
        {
            device.Id,
            device.Name,
            isDefault = string.Equals(profileRecord.DefaultDeviceId, device.Id, StringComparison.Ordinal)
                || string.Equals(profileRecord.DefaultDeviceName, device.Name, StringComparison.OrdinalIgnoreCase),
            device.SeenAtUtc,
        }).ToList();

        var lines = items.Count == 0
            ? ["No devices recorded for this profile."]
            : items.Select(item => $"{item.Name} ({item.Id}){(item.isDefault ? " [default]" : string.Empty)}").ToArray();
        return new CommandOutput(items, lines, warnings.Count == 0 ? null : warnings);
    }
}
