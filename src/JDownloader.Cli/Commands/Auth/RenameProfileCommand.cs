using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Auth;

public sealed class RenameProfileSettings : GlobalSettings
{
    [CommandArgument(0, "<OLD_NAME>")]
    public required string OldName { get; init; }

    [CommandArgument(1, "<NEW_NAME>")]
    public required string NewName { get; init; }
}

public sealed class RenameProfileCommand : AnonymousCommand<RenameProfileSettings>
{
    private readonly IProfileStore _profileStore;

    public RenameProfileCommand(IProfileStore profileStore, IOutputRenderer outputRenderer, IDiagnosticLogger diagnosticLogger)
        : base(outputRenderer, diagnosticLogger)
    {
        _profileStore = profileStore;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(CommandContext context, RenameProfileSettings settings, CancellationToken cancellationToken)
    {
        var oldName = settings.OldName.Trim();
        var newName = settings.NewName.Trim();
        var config = await _profileStore.LoadAsync(cancellationToken);
        if (!config.Profiles.TryGetValue(oldName, out var profile))
            throw CliException.NotFound($"Profile '{oldName}' was not found.");
        if (config.Profiles.ContainsKey(newName))
            throw CliException.Conflict($"Profile '{newName}' already exists.");

        if (settings.DryRun)
        {
            return new CommandOutput(
                new { action = "dry-run", oldName, newName },
                [
                    "Dry-run only. No changes were applied.",
                    $"Would rename profile '{oldName}' to '{newName}'.",
                ]);
        }

        config.Profiles.Remove(oldName);
        config.Profiles[newName] = profile;
        if (string.Equals(config.DefaultProfile, oldName, StringComparison.OrdinalIgnoreCase))
            config.DefaultProfile = newName;

        await _profileStore.SaveAsync(config, cancellationToken);
        return new CommandOutput(
            new { oldName, newName },
            [$"Renamed profile '{oldName}' to '{newName}'."]);
    }
}
