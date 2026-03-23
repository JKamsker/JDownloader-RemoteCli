using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Advanced;

public sealed class AdvancedDialogsGetSettings : DeviceCommandSettings
{
    [CommandOption("--id <ID>")]
    [Description("Dialog id to fetch.")]
    public long? Id { get; init; }

    [CommandOption("--icon")]
    [Description("Include dialog icon data where available.")]
    public bool Icon { get; init; }

    [CommandOption("--properties")]
    [Description("Include dialog properties where available.")]
    public bool Properties { get; init; }
}

public sealed class AdvancedDialogsGetCommand : DeviceApiCommand<AdvancedDialogsGetSettings>
{
    private readonly IMyJdTransport _transport;

    public AdvancedDialogsGetCommand(
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
        AdvancedDialogsGetSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.Id is null)
            throw CliException.Usage("advanced dialogs get requires --id <id>.");

        var plan = new MyJdRequestPlan(
            "advanced.dialogs.get",
            "POST",
            "/dialogs/get",
            new Dictionary<string, object?>
            {
                ["id"] = settings.Id.Value,
                ["icon"] = settings.Icon,
                ["properties"] = settings.Properties,
            },
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
