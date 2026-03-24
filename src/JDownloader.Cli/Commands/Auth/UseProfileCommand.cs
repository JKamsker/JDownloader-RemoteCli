using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Auth;

public sealed class UseProfileSettings : GlobalSettings
{
    [CommandArgument(0, "<NAME>")]
    [Description("Profile name to set as the default profile.")]
    public required string Name { get; init; }
}

public sealed class UseProfileCommand : AnonymousCommand<UseProfileSettings>
{
    private readonly IProfileStore _profileStore;

    public UseProfileCommand(IProfileStore profileStore, IOutputRenderer outputRenderer, IDiagnosticLogger diagnosticLogger)
        : base(outputRenderer, diagnosticLogger)
    {
        _profileStore = profileStore;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(CommandContext context, UseProfileSettings settings, CancellationToken cancellationToken)
    {
        var name = settings.Name.Trim();
        var config = await _profileStore.LoadAsync(cancellationToken);
        if (!config.Profiles.ContainsKey(name))
            throw CliException.NotFound($"Profile '{name}' was not found.");

        if (settings.DryRun)
        {
            return new CommandOutput(
                new { action = "dry-run", defaultProfile = name },
                [
                    "Dry-run only. No changes were applied.",
                    $"Would set default profile to '{name}'.",
                ]);
        }

        config.DefaultProfile = name;
        await _profileStore.SaveAsync(config, cancellationToken);
        return new CommandOutput(new { defaultProfile = name }, [$"Default profile set to '{name}'."]);
    }
}
