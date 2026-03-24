using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Events;

public sealed class EventsPollSettings : DeviceCommandSettings
{
    [CommandOption("--subscription-id <ID>")]
    [Description("Subscription id to poll.")]
    public long? SubscriptionId { get; init; }
}

public sealed class EventsPollCommand : DeviceApiCommand<EventsPollSettings>
{
    private readonly IMyJdTransport _transport;

    public EventsPollCommand(
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
        EventsPollSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.SubscriptionId is null)
            throw CliException.Usage("events poll requires --subscription-id <id>.");

        var plan = new MyJdRequestPlan(
            "events.poll",
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

