using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Auth;

public sealed class AddProfileSettings : GlobalSettings
{
    [CommandArgument(0, "<NAME>")]
    public required string Name { get; init; }
}

public sealed class AddProfileCommand : AnonymousCommand<AddProfileSettings>
{
    private readonly IProfileStore _profileStore;

    public AddProfileCommand(IProfileStore profileStore, IOutputRenderer outputRenderer, IDiagnosticLogger diagnosticLogger)
        : base(outputRenderer, diagnosticLogger)
    {
        _profileStore = profileStore;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(CommandContext context, AddProfileSettings settings, CancellationToken cancellationToken)
    {
        var name = settings.Name.Trim();
        var config = await _profileStore.LoadAsync(cancellationToken);
        if (config.Profiles.ContainsKey(name))
            throw CliException.Conflict($"Profile '{name}' already exists.");

        var wouldSetDefault = string.IsNullOrWhiteSpace(config.DefaultProfile);
        if (settings.DryRun)
        {
            return new CommandOutput(
                new
                {
                    action = "dry-run",
                    name,
                    wouldCreate = true,
                    wouldSetDefaultProfile = wouldSetDefault,
                },
                [
                    "Dry-run only. No changes were applied.",
                    $"Would create profile '{name}'.",
                    wouldSetDefault ? "Would set it as default profile." : "Would not change default profile.",
                ]);
        }

        config.Profiles[name] = new ProfileRecord
        {
            Output = settings.Output,
            TimeoutSeconds = settings.TimeoutSeconds,
        };
        config.DefaultProfile ??= name;
        await _profileStore.SaveAsync(config, cancellationToken);

        return new CommandOutput(
            new { name, created = true },
            [$"Created profile '{name}'."]);
    }
}
