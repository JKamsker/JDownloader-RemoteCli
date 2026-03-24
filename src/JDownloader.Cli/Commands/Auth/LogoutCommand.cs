using JDownloader.Cli.Auth;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Auth;

public sealed class LogoutCommand : ProfileApiCommand<NoArgSettings>
{
    private readonly IMyJdAuthService _authService;

    public LogoutCommand(
        IMyJdAuthService authService,
        IProfileResolver profileResolver,
        IOutputRenderer outputRenderer,
        IDiagnosticLogger diagnosticLogger)
        : base(profileResolver, outputRenderer, diagnosticLogger)
    {
        _authService = authService;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(
        CommandContext context,
        NoArgSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.DryRun)
        {
            return new CommandOutput(
                new { action = "dry-run", profile = resolved.ProfileName, loggedOut = true },
                [
                    "Dry-run only. No changes were applied.",
                    $"Profile: {resolved.ProfileName}",
                    "Would remove stored auth material.",
                ]);
        }

        await _authService.LogoutAsync(resolved.ProfileName, cancellationToken);
        return new CommandOutput(
            new { profile = resolved.ProfileName, loggedOut = true },
            [$"Removed stored auth for profile '{resolved.ProfileName}'."]);
    }
}
