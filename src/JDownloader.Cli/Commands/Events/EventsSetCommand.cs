using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Events;

public sealed class EventsSetSettings : DeviceCommandSettings
{
    [CommandOption("--subscription-id <ID>")]
    [Description("Subscription id to update.")]
    public long? SubscriptionId { get; init; }

    [CommandOption("--subscription <NAME>")]
    [Description("Repeatable publisher subscription name.")]
    public string[] Subscriptions { get; init; } = [];

    [CommandOption("--exclude <NAME>")]
    [Description("Repeatable exclusion pattern/name.")]
    public string[] Exclusions { get; init; } = [];
}

public sealed class EventsSetCommand : DeviceApiCommand<EventsSetSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public EventsSetCommand(
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
        EventsSetSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.SubscriptionId is null)
            throw CliException.Usage("events set requires --subscription-id <id>.");

        var subscriptions = settings.Subscriptions
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray();
        var exclusions = settings.Exclusions
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray();

        if (subscriptions.Length == 0 && exclusions.Length == 0)
            throw CliException.Usage("events set requires at least one --subscription <name> or --exclude <name>.");

        var plan = new MyJdRequestPlan(
            "events.set",
            "POST",
            "/events/setsubscription",
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

        await _confirmationGuard.AuthorizeAsync(settings, $"'events set' will update subscription {settings.SubscriptionId.Value}.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}

