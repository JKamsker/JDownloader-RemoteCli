using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Advanced;

public sealed class AdvancedDialogsTypeInfoSettings : DeviceCommandSettings
{
    [CommandOption("--dialog-type <TYPE>")]
    [Description("Dialog type to describe.")]
    public string? DialogType { get; init; }
}

public sealed class AdvancedDialogsTypeInfoCommand : DeviceApiCommand<AdvancedDialogsTypeInfoSettings>
{
    private readonly IMyJdTransport _transport;

    public AdvancedDialogsTypeInfoCommand(
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
        AdvancedDialogsTypeInfoSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.DialogType))
            throw CliException.Usage("advanced dialogs type-info requires --dialog-type <type>.");

        var plan = new MyJdRequestPlan(
            "advanced.dialogs.type-info",
            "POST",
            "/dialogs/getTypeInfo",
            new Dictionary<string, object?> { ["dialogType"] = settings.DialogType.Trim() },
            null,
            false,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(
            result.Data,
            HumanDataRenderer.Render(result.Data),
            result.Warnings);
    }
}
