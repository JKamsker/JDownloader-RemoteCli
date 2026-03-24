using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Accounts;

public sealed class AccountsHostersUrlSettings : DeviceCommandSettings
{
    [CommandOption("--hoster <NAME>")]
    [Description("Premium hoster name to resolve to its account URL.")]
    public string? Hoster { get; init; }
}

public sealed class AccountsHostersUrlCommand : DeviceApiCommand<AccountsHostersUrlSettings>
{
    private readonly IMyJdTransport _transport;

    public AccountsHostersUrlCommand(
        IProfileResolver profileResolver,
        IOutputRenderer outputRenderer,
        IDiagnosticLogger diagnosticLogger,
        IMyJdTransport transport)
        : base(profileResolver, outputRenderer, diagnosticLogger)
    {
        _transport = transport;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(
        CommandContext context,
        AccountsHostersUrlSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Hoster))
            throw CliException.Usage("accounts hosters url requires --hoster <name>.");

        var plan = new MyJdRequestPlan(
            "accounts.hosters.url",
            "POST",
            "/accountsV2/getPremiumHosterUrl",
            new Dictionary<string, object?> { ["hoster"] = settings.Hoster.Trim() },
            null,
            false,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var result = await _transport.ExecuteAsync(
            resolved,
            plan,
            cancellationToken);

        return new CommandOutput(
            result.Data,
            HumanDataRenderer.Render(result.Data),
            result.Warnings);
    }
}

