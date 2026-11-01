using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Connection;

internal static class ConnectionSummary
{
    public static object ToData(string name, ProfileRecord profile, bool isDefault)
    {
        var mode = ConnectionModes.Parse(profile.Mode);
        return new
        {
            name,
            isDefault,
            mode = ConnectionModes.ToName(mode),
            endpoint = mode == ConnectionMode.Direct ? Describe(profile) : null,
            host = mode == ConnectionMode.Direct ? profile.Host ?? DirectEndpoint.DefaultHost : null,
            port = mode == ConnectionMode.Direct ? profile.Port ?? DirectEndpoint.DefaultPort : (int?)null,
            useHttps = mode == ConnectionMode.Direct ? profile.UseHttps ?? false : (bool?)null,
            allowRemote = mode == ConnectionMode.Direct ? profile.AllowRemote ?? false : (bool?)null,
            accountEmail = profile.AccountEmail,
            timeoutSeconds = profile.TimeoutSeconds,
        };
    }

    public static string ToLine(string name, ProfileRecord profile, bool isDefault)
    {
        var mode = ConnectionModes.Parse(profile.Mode);
        var detail = mode == ConnectionMode.Direct
            ? Describe(profile)
            : $"email={profile.AccountEmail ?? "(none)"}, device={profile.DefaultDeviceName ?? profile.DefaultDeviceId ?? "(none)"}";
        return $"{name}{(isDefault ? " (default)" : string.Empty)} [{ConnectionModes.ToName(mode)}]: {detail}";
    }

    private static string Describe(ProfileRecord profile)
    {
        var scheme = profile.UseHttps == true ? "https" : "http";
        return $"{scheme}://{profile.Host ?? DirectEndpoint.DefaultHost}:{profile.Port ?? DirectEndpoint.DefaultPort}";
    }
}

public sealed class ConnectionListCommand : AnonymousCommand<NoArgSettings>
{
    private readonly IProfileStore _profileStore;

    public ConnectionListCommand(IProfileStore profileStore, IOutputRenderer outputRenderer, IDiagnosticLogger diagnosticLogger)
        : base(outputRenderer, diagnosticLogger)
    {
        _profileStore = profileStore;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(CommandContext context, NoArgSettings settings, CancellationToken cancellationToken)
    {
        var config = await _profileStore.LoadAsync(cancellationToken);
        var ordered = config.Profiles.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase).ToList();
        bool IsDefault(string name) => string.Equals(config.DefaultProfile, name, StringComparison.OrdinalIgnoreCase);

        var items = ordered.Select(pair => ConnectionSummary.ToData(pair.Key, pair.Value, IsDefault(pair.Key))).ToList();
        var lines = ordered.Count == 0
            ? ["No connections configured. Try 'jdr connection add local --mode direct'."]
            : ordered.Select(pair => ConnectionSummary.ToLine(pair.Key, pair.Value, IsDefault(pair.Key))).ToArray();
        return new CommandOutput(items, lines);
    }
}

public sealed class ConnectionGetSettings : GlobalSettings
{
    [CommandArgument(0, "<NAME>")]
    [Description("Connection (profile) name to inspect.")]
    public required string Name { get; init; }
}

public sealed class ConnectionGetCommand : AnonymousCommand<ConnectionGetSettings>
{
    private readonly IProfileStore _profileStore;

    public ConnectionGetCommand(IProfileStore profileStore, IOutputRenderer outputRenderer, IDiagnosticLogger diagnosticLogger)
        : base(outputRenderer, diagnosticLogger)
    {
        _profileStore = profileStore;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(CommandContext context, ConnectionGetSettings settings, CancellationToken cancellationToken)
    {
        var config = await _profileStore.LoadAsync(cancellationToken);
        var name = settings.Name.Trim();
        if (!config.Profiles.TryGetValue(name, out var profile))
            throw CliException.NotFound($"Connection '{name}' was not found.", "Run 'jdr connection list'.");

        var isDefault = string.Equals(config.DefaultProfile, name, StringComparison.OrdinalIgnoreCase);
        return new CommandOutput(
            ConnectionSummary.ToData(name, profile, isDefault),
            [ConnectionSummary.ToLine(name, profile, isDefault)]);
    }
}
