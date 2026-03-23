using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.System;

public sealed class SystemToggleSettings : DeviceCommandSettings
{
    [CommandArgument(0, "<NAME>")]
    [Description("Toggle name (e.g. pause-downloads, speed-limit, premium, clipboard-monitoring, automatic-reconnect, stop-after-current).")]
    public required string Name { get; init; }
}

public sealed class SystemToggleCommand : DeviceApiCommand<SystemToggleSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public SystemToggleCommand(
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
        SystemToggleSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        var name = settings.Name.Trim().ToLowerInvariant();
        var (operation, endpoint) = name switch
        {
            "automatic-reconnect" or "reconnect" => ("system.toggle.automatic-reconnect", "/toolbar/toggleAutomaticReconnect"),
            "clipboard" or "clipboard-monitoring" => ("system.toggle.clipboard-monitoring", "/toolbar/toggleClipboardMonitoring"),
            "speed-limit" or "download-speed-limit" => ("system.toggle.speed-limit", "/toolbar/toggleDownloadSpeedLimit"),
            "pause-downloads" or "pause" => ("system.toggle.pause-downloads", "/toolbar/togglePauseDownloads"),
            "premium" => ("system.toggle.premium", "/toolbar/togglePremium"),
            "stop-after-current" or "stop-after-current-download" => ("system.toggle.stop-after-current", "/toolbar/toggleStopAfterCurrentDownload"),
            _ => throw CliException.Usage($"Unsupported toggle name '{settings.Name}'."),
        };

        var plan = new MyJdRequestPlan(
            operation,
            "POST",
            endpoint,
            null,
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var proceed = await _confirmationGuard.AuthorizeAsync(settings, $"'system toggle {name}' will change remote state.");
        if (!proceed)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(
            result.Data,
            HumanDataRenderer.Render(result.Data),
            result.Warnings);
    }
}

