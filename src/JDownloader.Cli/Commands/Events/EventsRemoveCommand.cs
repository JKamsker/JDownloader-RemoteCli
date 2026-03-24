using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Events;

public sealed class EventsRemoveSettings : DeviceCommandSettings
{
    [CommandOption("--subscription-id <ID>")]
    [Description("Subscription id to update.")]
    public long? SubscriptionId { get; init; }

    [CommandOption("--subscription <NAME>")]
    [Description("Repeatable publisher subscription name to remove.")]
    public string[] Subscriptions { get; init; } = [];

    [CommandOption("--exclude <NAME>")]
    [Description("Repeatable exclusion pattern/name to remove.")]
    public string[] Exclusions { get; init; } = [];
}

public sealed class EventsRemoveCommand : DeviceApiCommand<EventsRemoveSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public EventsRemoveCommand(
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
        EventsRemoveSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.SubscriptionId is null)
            throw CliException.Usage("events remove requires --subscription-id <id>.");

        var subscriptions = settings.Subscriptions
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray();
        var exclusions = settings.Exclusions
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray();

        if (subscriptions.Length == 0 && exclusions.Length == 0)
            throw CliException.Usage("events remove requires at least one --subscription <name> or --exclude <name>.");

        var plan = new MyJdRequestPlan(
            "events.remove",
            "POST",
            "/events/removesubscription",
            new Dictionary<string, object?>
            {
                ["subscriptionid"] = settings.SubscriptionId.Value,
                ["subscriptions"] = subscriptions,
                ["exclusions"] = exclusions,
            },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(settings, $"'events remove' will update subscription {settings.SubscriptionId.Value}.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}

