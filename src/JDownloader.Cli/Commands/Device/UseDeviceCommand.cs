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

public sealed class UseDeviceCommand : AnonymousCommand<UseDeviceSettings>
{
    private readonly IProfileResolver _profileResolver;
    private readonly IProfileStore _profileStore;
    private readonly IDeviceCatalog _deviceCatalog;

    public UseDeviceCommand(
        IProfileResolver profileResolver,
        IDeviceCatalog deviceCatalog,
        IProfileStore profileStore,
        IOutputRenderer outputRenderer,
        IDiagnosticLogger diagnosticLogger)
        : base(outputRenderer, diagnosticLogger)
    {
        _profileResolver = profileResolver;
        _deviceCatalog = deviceCatalog;
        _profileStore = profileStore;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(CommandContext context, UseDeviceSettings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Device))
            throw CliException.Usage("device use requires --device <id-or-name>.");

        var deviceValue = settings.Device.Trim();
        var resolved = await _profileResolver.ResolveAsync(settings, requireDevice: false, resolveDeviceSelectors: false, cancellationToken);
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

        var match = FindMatch(profileRecord, deviceValue);
        if (match is null && !string.IsNullOrWhiteSpace(profileRecord.AccountEmail))
        {
            try
            {
                await _deviceCatalog.SyncAsync(resolved.ProfileName, profileRecord.AccountEmail, resolved.TimeoutSeconds, cancellationToken);
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

                match = FindMatch(profileRecord, deviceValue);
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
        await _profileStore.SaveAsync(config, cancellationToken);

        return new CommandOutput(
            new { profile = resolved.ProfileName, device = new { selected.Id, selected.Name } },
            [$"Default device for profile '{resolved.ProfileName}' set to {selected.Name} ({selected.Id})."]);
    }

    private static KnownDeviceRecord? FindMatch(ProfileRecord profile, string lookup)
    {
        return profile.KnownDevices.FirstOrDefault(device =>
            string.Equals(device.Id, lookup, StringComparison.Ordinal)
            || string.Equals(device.Name, lookup, StringComparison.Ordinal)
            || string.Equals(device.Name, lookup, StringComparison.OrdinalIgnoreCase));
    }
}
