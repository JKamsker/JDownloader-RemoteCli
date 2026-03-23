using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Grabber;

public sealed class GrabberAddContainerSettings : DeviceCommandSettings
{
    [CommandOption("--type <TYPE>")]
    [Description("Container type (e.g., DLC, CCF, RSDF).")]
    public string? Type { get; init; }

    [CommandOption("--content <CONTENT>")]
    [Description("Container content (payload string).")]
    public string? Content { get; init; }

    [CommandOption("--content-file <PATH>")]
    [Description("Read container content from a local file.")]
    public string? ContentFile { get; init; }
}

public sealed class GrabberAddContainerCommand : DeviceApiCommand<GrabberAddContainerSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public GrabberAddContainerCommand(
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
        GrabberAddContainerSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Type))
            throw CliException.Usage("grabber add-container requires --type <type>.");

        if (!string.IsNullOrWhiteSpace(settings.Content) && !string.IsNullOrWhiteSpace(settings.ContentFile))
            throw CliException.Usage("grabber add-container requires exactly one of --content <content> or --content-file <path>.");

        var content = !string.IsNullOrWhiteSpace(settings.ContentFile)
            ? await File.ReadAllTextAsync(settings.ContentFile.Trim(), cancellationToken)
            : settings.Content;

        if (string.IsNullOrWhiteSpace(content))
            throw CliException.Usage("grabber add-container requires --content <content> (or --content-file <path>) with non-empty content.");

        var plan = new MyJdRequestPlan(
            "grabber.add-container",
            "POST",
            "/linkgrabberv2/addContainer",
            new Dictionary<string, object?>
            {
                ["type"] = settings.Type.Trim(),
                ["content"] = content.Trim(),
            },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var proceed = await _confirmationGuard.AuthorizeAsync(settings, "'grabber add-container' will add a container to the linkgrabber.");
        if (!proceed)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}

