using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Grabber;

public sealed class GrabberMoveToDownloadsSettings : DeviceCommandSettings
{
    [CommandOption("--link-id <ID>")]
    [Description("Repeatable linkgrabber link identifier to move.")]
    public long[] LinkIds { get; init; } = [];

    [CommandOption("--package-id <ID>")]
    [Description("Repeatable linkgrabber package identifier to move.")]
    public long[] PackageIds { get; init; } = [];
}

public sealed class GrabberMoveToDownloadsCommand : DeviceApiCommand<GrabberMoveToDownloadsSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public GrabberMoveToDownloadsCommand(
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
        GrabberMoveToDownloadsSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        var plan = new MyJdRequestPlan(
            "grabber.move-to-downloads",
            "POST",
            "/linkgrabberv2/moveToDownloadlist",
            new Dictionary<string, object?>
            {
                ["linkIds"] = settings.LinkIds,
                ["packageIds"] = settings.PackageIds,
            },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        if (settings.LinkIds.Length == 0 && settings.PackageIds.Length == 0)
            throw CliException.Usage("grabber move-to-downloads requires at least one --link-id <id> or --package-id <id>.");

        await _confirmationGuard.AuthorizeAsync(settings, "'grabber move-to-downloads' will move selected items into downloads.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(
            result.Data,
            HumanDataRenderer.Render(result.Data),
            result.Warnings);
    }
}
