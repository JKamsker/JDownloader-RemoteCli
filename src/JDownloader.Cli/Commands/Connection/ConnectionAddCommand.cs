using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Connection;

public sealed class ConnectionAddSettings : GlobalSettings
{
    [CommandArgument(0, "<NAME>")]
    [Description("Connection (profile) name to create or update.")]
    public required string Name { get; init; }

    [CommandOption("--mode <MODE>")]
    [Description("Connection mode: 'direct' (local JDownloader HTTP API, no account) or 'myjd' (My.JDownloader cloud). Default: myjd.")]
    public string? Mode { get; init; }

    [CommandOption("--host <HOST>")]
    [Description("Direct mode: hostname or IP of the JDownloader machine. Default: 127.0.0.1.")]
    public string? Host { get; init; }

    [CommandOption("--port <PORT>")]
    [Description("Direct mode: port of JDownloader's local Remote API. Default: 3128.")]
    public int? Port { get; init; }

    [CommandOption("--https")]
    [Description("Direct mode: use https (only when JDownloader sits behind a TLS reverse proxy).")]
    public bool Https { get; init; }

    [CommandOption("--allow-remote")]
    [Description("Direct mode: explicitly allow a non-loopback host. The local API is unauthenticated; see the security notes.")]
    public bool AllowRemote { get; init; }

    [CommandOption("--force")]
    [Description("Replace the connection settings of an existing profile.")]
    public bool Force { get; init; }
}

public sealed class ConnectionAddCommand : AnonymousCommand<ConnectionAddSettings>
{
    private readonly IProfileStore _profileStore;

    public ConnectionAddCommand(IProfileStore profileStore, IOutputRenderer outputRenderer, IDiagnosticLogger diagnosticLogger)
        : base(outputRenderer, diagnosticLogger)
    {
        _profileStore = profileStore;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(CommandContext context, ConnectionAddSettings settings, CancellationToken cancellationToken)
    {
        var name = settings.Name.Trim();
        var mode = ConnectionModes.Parse(settings.Mode);
        var config = await _profileStore.LoadAsync(cancellationToken);
        var exists = config.Profiles.TryGetValue(name, out var existing);
        if (exists && !settings.Force)
            throw CliException.Conflict($"Connection '{name}' already exists.", "Pass --force to replace its connection settings.");

        var warnings = new List<string>();
        var updated = existing ?? new ProfileRecord { Output = settings.Output, TimeoutSeconds = settings.TimeoutSeconds };
        if (mode == ConnectionMode.Direct)
            ApplyDirect(name, settings, updated, warnings);
        else
            ApplyMyJd(settings, updated);

        if (settings.TimeoutSeconds is > 0)
            updated.TimeoutSeconds = settings.TimeoutSeconds;

        var setDefault = string.IsNullOrWhiteSpace(config.DefaultProfile);
        var data = new
        {
            action = settings.DryRun ? "dry-run" : (exists ? "updated" : "created"),
            name,
            mode = ConnectionModes.ToName(mode),
            updated.Host,
            updated.Port,
            updated.UseHttps,
            updated.AllowRemote,
            setDefault,
        };

        var lines = new List<string>();
        if (settings.DryRun)
            lines.Add("Dry-run only. No changes were applied.");
        lines.Add($"{(exists ? "Updated" : "Created")} connection '{name}' ({ConnectionModes.ToName(mode)}).");
        if (mode == ConnectionMode.Direct)
            lines.Add($"Endpoint: {DirectEndpoint.FromProfile(name, updated).DisplayValue}");
        if (setDefault)
            lines.Add("Set as default connection.");
        if (mode == ConnectionMode.MyJd)
            lines.Add($"Next: 'jdr auth login --profile {name} --email <email> --password-stdin'.");

        if (settings.DryRun)
            return new CommandOutput(data, lines, warnings.Count == 0 ? null : warnings);

        config.Profiles[name] = updated;
        if (setDefault)
            config.DefaultProfile = name;
        await _profileStore.SaveAsync(config, cancellationToken);
        return new CommandOutput(data, lines, warnings.Count == 0 ? null : warnings);
    }

    private static void ApplyDirect(string name, ConnectionAddSettings settings, ProfileRecord profile, List<string> warnings)
    {
        var host = string.IsNullOrWhiteSpace(settings.Host) ? DirectEndpoint.DefaultHost : settings.Host.Trim();
        var port = settings.Port ?? DirectEndpoint.DefaultPort;
        DirectEndpoint.Validate(host, port);

        var loopback = DirectEndpoint.IsLoopbackHost(host);
        if (!loopback && !settings.AllowRemote)
        {
            throw CliException.Usage(
                $"Host '{host}' is not a loopback address. JDownloader's local API has no authentication and uses plain HTTP.",
                "Pass --allow-remote to confirm you really want to expose control of JDownloader on that network path (prefer an SSH tunnel to 127.0.0.1).");
        }

        if (settings.AllowRemote && loopback)
            warnings.Add("--allow-remote has no effect for a loopback host.");
        if (!loopback)
            warnings.Add(ConnectionWarnings.RemoteHost(host, port, settings.Https));

        profile.Mode = ConnectionModes.DirectName;
        profile.Host = host;
        profile.Port = port;
        profile.UseHttps = settings.Https ? true : null;
        profile.AllowRemote = !loopback ? true : null;
        profile.AccountEmail = null;
        profile.DefaultDeviceId = null;
        profile.DefaultDeviceName = null;
        profile.KnownDevices = [];
    }

    private static void ApplyMyJd(ConnectionAddSettings settings, ProfileRecord profile)
    {
        if (!string.IsNullOrWhiteSpace(settings.Host) || settings.Port is not null || settings.Https || settings.AllowRemote)
            throw CliException.Usage("--host, --port, --https and --allow-remote only apply to '--mode direct'.");

        profile.Mode = null;
        profile.Host = null;
        profile.Port = null;
        profile.UseHttps = null;
        profile.AllowRemote = null;
    }
}
