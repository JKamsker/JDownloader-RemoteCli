using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Events;

public sealed class EventsListenSettings : DeviceCommandSettings
{
    [CommandOption("--subscription-id <ID>")]
    [Description("Subscription id to listen on.")]
    public long? SubscriptionId { get; init; }
}

public sealed class EventsListenCommand : DeviceApiCommand<EventsListenSettings>
{
    private readonly IMyJdTransport _transport;

    public EventsListenCommand(
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
        EventsListenSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.SubscriptionId is null)
            throw CliException.Usage("events listen requires --subscription-id <id>.");

        var plan = new MyJdRequestPlan(
            "events.listen",
            "POST",
            "/events/listen",
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

