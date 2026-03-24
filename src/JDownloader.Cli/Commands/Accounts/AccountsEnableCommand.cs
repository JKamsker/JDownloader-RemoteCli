using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Accounts;

public sealed class AccountsEnableSettings : DeviceCommandSettings
{
    [CommandOption("--account-id <ID>")]
    [Description("Repeatable account identifier to enable.")]
    public long[] AccountIds { get; init; } = [];
}

public sealed class AccountsEnableCommand : DeviceApiCommand<AccountsEnableSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public AccountsEnableCommand(
        IProfileResolver profileResolver,
        IOutputRenderer outputRenderer,
        IDiagnosticLogger diagnosticLogger,
        IMyJdTransport transport,
        IConfirmationGuard confirmationGuard)
        : base(profileResolver, outputRenderer, diagnosticLogger)
    {
        _transport = transport;
        _confirmationGuard = confirmationGuard;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(
        CommandContext context,
        AccountsEnableSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.AccountIds.Length == 0)
            throw CliException.Usage("accounts enable requires at least one --account-id <id>.");

        var plan = new MyJdRequestPlan(
            "accounts.enable",
            "POST",
            "/accountsV2/enableAccounts",
            new Dictionary<string, object?> { ["ids"] = settings.AccountIds },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(
            settings,
            $"'accounts enable' will enable {settings.AccountIds.Length} account(s).");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(
            result.Data,
            HumanDataRenderer.Render(result.Data),
            result.Warnings);
    }
}
