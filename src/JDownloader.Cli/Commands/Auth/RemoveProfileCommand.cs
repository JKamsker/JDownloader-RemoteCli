using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Auth;

public sealed class RemoveProfileSettings : GlobalSettings
{
    [CommandArgument(0, "<NAME>")]
    [Description("Profile name to remove.")]
    public required string Name { get; init; }
}

public sealed class RemoveProfileCommand : AnonymousCommand<RemoveProfileSettings>
{
    private readonly IProfileStore _profileStore;
    private readonly IConfirmationGuard _confirmationGuard;

    public RemoveProfileCommand(
        IProfileStore profileStore,
        IConfirmationGuard confirmationGuard,
        IOutputRenderer outputRenderer,
        IDiagnosticLogger diagnosticLogger)
        : base(outputRenderer, diagnosticLogger)
    {
        _profileStore = profileStore;
        _confirmationGuard = confirmationGuard;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(CommandContext context, RemoveProfileSettings settings, CancellationToken cancellationToken)
    {
        var name = settings.Name.Trim();
        var config = await _profileStore.LoadAsync(cancellationToken);
        if (!config.Profiles.TryGetValue(name, out var profile))
            throw CliException.NotFound($"Profile '{name}' was not found.");

        var accountEmail = profile?.AccountEmail;
        var wouldRemoveCredentials = !string.IsNullOrWhiteSpace(accountEmail)
            && !config.Profiles.Any(pair => !string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(pair.Value?.AccountEmail)
                && string.Equals(pair.Value!.AccountEmail, accountEmail, StringComparison.OrdinalIgnoreCase));

        var wouldUpdateDefaultProfile = string.Equals(config.DefaultProfile, name, StringComparison.OrdinalIgnoreCase);
        var nextDefault = wouldUpdateDefaultProfile
            ? config.Profiles.Keys
                .Where(candidate => !string.Equals(candidate, name, StringComparison.OrdinalIgnoreCase))
                .OrderBy(candidate => candidate, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault()
            : config.DefaultProfile;

        if (settings.DryRun)
        {
            return new CommandOutput(
                new
                {
                    action = "dry-run",
                    profile = name,
                    wouldRemoveCredentials,
                    nextDefaultProfile = nextDefault,
                },
                [
                    "Dry-run only. No changes were applied.",
                    $"Would remove profile '{name}'.",
                    wouldUpdateDefaultProfile
                        ? $"Would update default profile to '{nextDefault ?? "(none)"}'."
                        : "Would not change default profile.",
                    wouldRemoveCredentials
                        ? "Would remove stored credentials for the profile email."
                        : "Would keep stored credentials (shared by another profile).",
                ]);
        }

        await _confirmationGuard.AuthorizeAsync(settings, $"Remove profile '{name}'?");

        config.Profiles.Remove(name);
        if (wouldUpdateDefaultProfile)
            config.DefaultProfile = nextDefault;

        if (wouldRemoveCredentials)
            config.Credentials.Remove(accountEmail!);

        await _profileStore.SaveAsync(config, cancellationToken);
        return new CommandOutput(new { profile = name, removed = true }, [$"Removed profile '{name}'."]);
    }
}
