using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Auth;

public sealed class WhoAmICommand : ProfileApiCommand<NoArgSettings>
{
    public WhoAmICommand(
        IProfileResolver profileResolver,
        IOutputRenderer outputRenderer,
        IDiagnosticLogger diagnosticLogger)
        : base(profileResolver, outputRenderer, diagnosticLogger)
    {
    }

    protected override Task<CommandOutput> ExecuteCoreAsync(
        CommandContext context,
        NoArgSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new CommandOutput(
            new
            {
                profile = resolved.ProfileName,
                resolved.AccountEmail,
                mode = ConnectionModes.ToName(resolved.ConnectionMode),
                endpoint = resolved.Direct?.DisplayValue,
                resolved.ProfileSource,
                resolved.OutputMode,
                resolved.TimeoutSeconds,
            },
            [
                $"Profile: {resolved.ProfileName}",
                resolved.IsDirect
                    ? $"Mode: direct ({resolved.Direct?.DisplayValue})"
                    : $"Email: {resolved.AccountEmail ?? "(none)"}",
                $"Profile source: {resolved.ProfileSource}",
                $"Output mode: {resolved.OutputMode}",
                $"Timeout: {resolved.TimeoutSeconds}s",
            ]));
    }
}
