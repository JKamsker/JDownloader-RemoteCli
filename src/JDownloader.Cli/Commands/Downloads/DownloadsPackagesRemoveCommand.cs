using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Downloads;

public sealed class DownloadsPackagesRemoveSettings : DeviceCommandSettings
{
    [CommandOption("--package-id <ID>")]
    [Description("Repeatable download package identifier to remove.")]
    public long[] PackageIds { get; init; } = [];
}

public sealed class DownloadsPackagesRemoveCommand : DeviceApiCommand<DownloadsPackagesRemoveSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public DownloadsPackagesRemoveCommand(
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
        DownloadsPackagesRemoveSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.PackageIds.Length == 0)
            throw CliException.Usage("downloads packages remove requires at least one --package-id <id>.");

        var plan = new MyJdRequestPlan(
            "downloads.packages.remove",
            "POST",
            "/downloadsV2/removeLinks",
            new Dictionary<string, object?> { ["packageIds"] = settings.PackageIds },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(settings, "'downloads packages remove' will remove selected download packages.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}
