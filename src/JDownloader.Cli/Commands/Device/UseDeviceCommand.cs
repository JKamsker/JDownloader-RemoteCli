using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Device;

public sealed class UseDeviceSettings : GlobalSettings
{
    [CommandOption("--device-name <NAME>")]
    [Description("Optional friendly name when adding a new local device record.")]
    public string? DeviceName { get; init; }
}

public sealed class UseDeviceCommand : ProfileApiCommand<UseDeviceSettings>
{
    private readonly IProfileStore _profileStore;
    private readonly IDeviceCatalog _deviceCatalog;

    public UseDeviceCommand(
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
        UseDeviceSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Device))
            throw CliException.Usage("device use requires --device <id-or-name>.");

        var deviceValue = settings.Device.Trim();
        var config = await _profileStore.LoadAsync(cancellationToken);

        ProfileRecord profileRecord;
        if (!config.Profiles.TryGetValue(resolved.ProfileName, out var profile) || profile is null)
        {
            profileRecord = new ProfileRecord();
            config.Profiles[resolved.ProfileName] = profileRecord;
        }
        else
        {
            profileRecord = profile;
        }

        var match = FindMatch(profileRecord, deviceValue, source: "local cache");
        if (match is null && !string.IsNullOrWhiteSpace(profileRecord.AccountEmail))
        {
            try
            {
                var liveDevices = await _deviceCatalog.SyncAsync(
                    resolved.ProfileName,
                    profileRecord.AccountEmail,
                    resolved.TimeoutSeconds,
                    persist: !settings.DryRun,
                    cancellationToken);

                if (!settings.DryRun)
                    config = await _profileStore.LoadAsync(cancellationToken);

                if (!config.Profiles.TryGetValue(resolved.ProfileName, out profile) || profile is null)
                {
                    profileRecord = new ProfileRecord();
                    config.Profiles[resolved.ProfileName] = profileRecord;
                }
                else
                {
                    profileRecord = profile;
                }

                if (settings.DryRun)
                {
                    var liveProfile = new ProfileRecord
                    {
                        KnownDevices = liveDevices.Select(device => new KnownDeviceRecord
                        {
                            Id = device.Id,
                            Name = device.Name,
                            SeenAtUtc = DateTimeOffset.UtcNow,
                        }).ToList(),
                    };
                    match = FindMatch(liveProfile, deviceValue, source: "live sync");
                }
                else
                {
                    match = FindMatch(profileRecord, deviceValue, source: "live sync");
                }
            }
            catch (CliException ex) when (ex.Kind is "not_authenticated" or "transport")
            {
                // keep the original ergonomic fallback and let the caller opt into a manual record
            }
        }

        KnownDeviceRecord selected;
        if (match is null)
        {
            selected = new KnownDeviceRecord
            {
                Id = deviceValue,
                Name = settings.DeviceName?.Trim() ?? deviceValue,
                SeenAtUtc = DateTimeOffset.UtcNow,
            };
            profileRecord.KnownDevices.Add(selected);
        }
        else
        {
            selected = match;
        }

        profileRecord.DefaultDeviceId = selected.Id;
        profileRecord.DefaultDeviceName = selected.Name;
        if (settings.DryRun)
        {
            return new CommandOutput(
                new
                {
                    action = "dry-run",
                    profile = resolved.ProfileName,
                    device = new { selected.Id, selected.Name },
                    wouldPersist = true,
                },
                [
                    "Dry-run only. No changes were applied.",
                    $"Profile: {resolved.ProfileName}",
                    $"Would set default device to {selected.Name} ({selected.Id}).",
                ]);
        }

        await _profileStore.SaveAsync(config, cancellationToken);

        return new CommandOutput(
            new { profile = resolved.ProfileName, device = new { selected.Id, selected.Name } },
            [$"Default device for profile '{resolved.ProfileName}' set to {selected.Name} ({selected.Id})."]);
    }

    private static KnownDeviceRecord? FindMatch(ProfileRecord profile, string lookup, string source)
    {
        var byId = profile.KnownDevices.Where(device => string.Equals(device.Id, lookup, StringComparison.Ordinal)).ToList();
        if (byId.Count == 1)
            return byId[0];
        if (byId.Count > 1)
            throw CliException.Usage($"Device value '{lookup}' is ambiguous in {source} resolution.");

        var byName = profile.KnownDevices.Where(device => string.Equals(device.Name, lookup, StringComparison.Ordinal)).ToList();
        if (byName.Count == 1)
            return byName[0];
        if (byName.Count > 1)
            throw CliException.Usage($"Device name '{lookup}' is ambiguous in {source} resolution.");

        var byCaseInsensitiveName = profile.KnownDevices.Where(device => string.Equals(device.Name, lookup, StringComparison.OrdinalIgnoreCase)).ToList();
        if (byCaseInsensitiveName.Count == 1)
            return byCaseInsensitiveName[0];
        if (byCaseInsensitiveName.Count > 1)
            throw CliException.Usage($"Device name '{lookup}' matches multiple devices in {source} resolution.");

        return null;
    }
}
