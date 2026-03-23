using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.System;

public sealed class SystemOsShutdownSettings : DeviceCommandSettings
{
    [CommandOption("--force")]
    [Description("Force OS shutdown (matches /system/shutdownOS?force parameter).")]
    public bool Force { get; init; }
}

public sealed class SystemOsShutdownCommand : DeviceApiCommand<SystemOsShutdownSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public SystemOsShutdownCommand(
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
        SystemOsShutdownSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        var plan = new MyJdRequestPlan(
            "system.os.shutdown",
            "POST",
            "/system/shutdownOS",
            new Dictionary<string, object?> { ["force"] = settings.Force },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var proceed = await _confirmationGuard.AuthorizeAsync(
            settings,
            $"'system os shutdown' will shut down the OS on the selected device (force={settings.Force.ToString().ToLowerInvariant()}).");
        if (!proceed)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}
