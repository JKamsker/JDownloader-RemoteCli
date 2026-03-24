using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Events;

public sealed class EventsSubscribeSettings : DeviceCommandSettings
{
    [CommandOption("--subscription <NAME>")]
    [Description("Repeatable publisher subscription name.")]
    public string[] Subscriptions { get; init; } = [];

    [CommandOption("--exclude <NAME>")]
    [Description("Repeatable exclusion pattern/name.")]
    public string[] Exclusions { get; init; } = [];
}

public sealed class EventsSubscribeCommand : DeviceApiCommand<EventsSubscribeSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public EventsSubscribeCommand(
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
        EventsSubscribeSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        var subscriptions = settings.Subscriptions
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray();
        var exclusions = settings.Exclusions
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray();

        if (subscriptions.Length == 0)
            throw CliException.Usage("events subscribe requires at least one --subscription <name>.");

        var plan = new MyJdRequestPlan(
            "events.subscribe",
            "POST",
            "/events/subscribe",
            new Dictionary<string, object?>
            {
                ["subscriptions"] = subscriptions,
                ["exclusions"] = exclusions,
            },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(settings, $"'events subscribe' will create a subscription for {subscriptions.Length} publisher(s).");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}

