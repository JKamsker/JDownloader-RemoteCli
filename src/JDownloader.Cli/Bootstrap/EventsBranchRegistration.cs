using JDownloader.Cli.Commands.Events;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class CliApplicationEventsRegistration
{
    public static void Register(IConfigurator config)
    {
        config.AddBranch("events", events =>
        {
            events.SetDescription("Inspect and manage event subscriptions.");
            events.AddCommand<EventsPublishersCommand>("publishers").WithDescription("List event publishers.");
            events.AddCommand<EventsSubscribeCommand>("subscribe").WithDescription("Create a new subscription.");
            events.AddCommand<EventsSetCommand>("set").WithDescription("Set subscription content.");
            events.AddCommand<EventsRemoveCommand>("remove").WithDescription("Remove subscription content.");
            events.AddCommand<EventsStatusCommand>("status").WithDescription("Get subscription status.");
            events.AddCommand<EventsListenCommand>("listen").WithDescription("Listen for events on a subscription id.");
        });
    }
}
