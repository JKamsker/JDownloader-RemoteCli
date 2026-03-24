using JDownloader.Cli.Commands.Grabber;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class CliApplicationGrabberRegistration
{
    public static void Register(IConfigurator config)
    {
        config.AddBranch("grabber", grabber =>
        {
            grabber.SetDescription("Manage linkgrabber ingestion and staging.");
            grabber.AddCommand<GrabberAddCommand>("add").WithDescription("Add links to the linkgrabber.");
            grabber.AddCommand<GrabberAddContainerCommand>("add-container").WithDescription("Add a container file to the linkgrabber.");
            grabber.AddCommand<GrabberClearCommand>("clear").WithDescription("Clear the linkgrabber list (0-arg endpoint).");
            grabber.AddCommand<GrabberMoveToDownloadsCommand>("move-to-downloads").WithDescription("Move linkgrabber selection into downloads.");
            grabber.AddBranch("links", links =>
            {
                links.SetDescription("Inspect and manage linkgrabber links.");
                links.AddCommand<GrabberLinksListCommand>("list").WithDescription("List linkgrabber links.");
                links.AddCommand<GrabberLinksRemoveCommand>("remove").WithDescription("Remove linkgrabber links by id.");
            });
            grabber.AddBranch("packages", packages =>
            {
                packages.SetDescription("Inspect and manage linkgrabber packages.");
                packages.AddCommand<GrabberPackagesListCommand>("list").WithDescription("List linkgrabber packages.");
                packages.AddCommand<GrabberPackagesRemoveCommand>("remove").WithDescription("Remove linkgrabber packages by id.");
            });
            grabber.AddBranch("jobs", jobs =>
            {
                jobs.SetDescription("Inspect crawler jobs.");
                jobs.AddCommand<GrabberJobsListCommand>("list").WithDescription("List crawler jobs.");
                jobs.AddCommand<GrabberJobsGetCommand>("get").WithDescription("Get crawler jobs by id.");
            });
            grabber.AddBranch("variants", variants =>
            {
                variants.SetDescription("Inspect and set variants for crawl results.");
                variants.AddCommand<GrabberVariantsListCommand>("list").WithDescription("List variants for the current selection.");
                variants.AddCommand<GrabberVariantsSetCommand>("set").WithDescription("Select a variant for a linkgrabber link.");
            });
        });
    }
}
