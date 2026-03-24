using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Events;

public sealed class EventsStatusSettings : DeviceCommandSettings
{
    [CommandOption("--subscription-id <ID>")]
    [Description("Subscription id to inspect.")]
    public long? SubscriptionId { get; init; }
}

public sealed class EventsStatusCommand : DeviceApiCommand<EventsStatusSettings>
{
    private readonly IMyJdTransport _transport;

    public EventsStatusCommand(
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
        EventsStatusSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.SubscriptionId is null)
            throw CliException.Usage("events status requires --subscription-id <id>.");

        var plan = new MyJdRequestPlan(
            "events.status",
            "POST",
            "/events/getsubscriptionstatus",
            new Dictionary<string, object?> { ["subscriptionid"] = settings.SubscriptionId.Value },
            null,
            false,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}

