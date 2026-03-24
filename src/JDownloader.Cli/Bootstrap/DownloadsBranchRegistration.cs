using JDownloader.Cli.Commands.Downloads;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class CliApplicationDownloadsRegistration
{
    public static void Register(IConfigurator config)
    {
        config.AddBranch("downloads", downloads =>
        {
            downloads.SetDescription("Inspect and control active downloads.");
            downloads.AddCommand<DownloadsStatusCommand>("status").WithDescription("Show download controller status.");
            downloads.AddCommand<DownloadsSpeedCommand>("speed").WithDescription("Show current aggregated download speed.");
            downloads.AddCommand<DownloadsStartCommand>("start").WithDescription("Start downloads.");
            downloads.AddCommand<DownloadsStopCommand>("stop").WithDescription("Stop downloads.");
            downloads.AddCommand<DownloadsPauseCommand>("pause").WithDescription("Pause downloads (or resume with --resume).");
            downloads.AddBranch("links", links =>
            {
                links.SetDescription("Inspect and manage download links.");
                links.AddCommand<DownloadsLinksListCommand>("list").WithDescription("List download links.");
                links.AddCommand<DownloadsLinksRemoveCommand>("remove").WithDescription("Remove download links by id.");
            });
            downloads.AddBranch("packages", packages =>
            {
                packages.SetDescription("Inspect and manage download packages.");
                packages.AddCommand<DownloadsPackagesListCommand>("list").WithDescription("List download packages.");
                packages.AddCommand<DownloadsPackagesRemoveCommand>("remove").WithDescription("Remove download packages by id.");
            });
            downloads.AddBranch("stopmark", stopmark =>
            {
                stopmark.SetDescription("Inspect and manage download stopmarks.");
                stopmark.AddCommand<DownloadsStopmarkGetCommand>("get").WithDescription("Get the current stopmark.");
                stopmark.AddCommand<DownloadsStopmarkSetCommand>("set").WithDescription("Set the stopmark (requires link + package id).");
                stopmark.AddCommand<DownloadsStopmarkClearCommand>("clear").WithDescription("Clear the stopmark.");
            });
        });
    }
}
