using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Downloads;

public sealed class DownloadsPauseSettings : DeviceCommandSettings
{
    [CommandOption("--resume")]
    [Description("Resume downloads instead of pausing them.")]
    public bool Resume { get; init; }
}

public sealed class DownloadsPauseCommand : DeviceApiCommand<DownloadsPauseSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public DownloadsPauseCommand(
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
        DownloadsPauseSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        var value = !settings.Resume;
        var plan = new MyJdRequestPlan(
            value ? "downloads.pause" : "downloads.resume",
            "POST",
            "/downloadcontroller/pause",
            new Dictionary<string, object?> { ["value"] = value },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var proceed = await _confirmationGuard.AuthorizeAsync(
            settings,
            value ? "'downloads pause' will pause downloads." : "'downloads pause --resume' will resume downloads.");
        if (!proceed)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(
            result.Data,
            HumanDataRenderer.Render(result.Data),
            result.Warnings);
    }
}

