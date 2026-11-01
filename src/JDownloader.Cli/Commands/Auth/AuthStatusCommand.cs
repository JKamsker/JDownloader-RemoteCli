using JDownloader.Cli.Auth;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Auth;

public sealed class AuthStatusCommand : ProfileApiCommand<NoArgSettings>
{
    private readonly IMyJdAuthService _authService;

    public AuthStatusCommand(
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
        if (resolved.IsDirect)
        {
            return new CommandOutput(
                new
                {
                    profile = resolved.ProfileName,
                    mode = ConnectionModes.DirectName,
                    endpoint = resolved.Direct?.DisplayValue,
                    authRequired = false,
                },
                [
                    $"Profile: {resolved.ProfileName}",
                    $"Mode: direct ({resolved.Direct?.DisplayValue})",
                    "Authentication: not required (JDownloader's local API is unauthenticated).",
                ]);
        }

        var status = await _authService.GetStatusAsync(resolved.ProfileName, cancellationToken);
        return new CommandOutput(
            status,
            [
                $"Profile: {status.ProfileName}",
                $"Email: {status.Email ?? "(none)"}",
                $"Stored auth: {(status.HasStoredAuth ? "yes" : "no")}",
                $"Relay transport ready: {(status.TransportReady ? "yes" : "no")}",
            ]);
    }
}
