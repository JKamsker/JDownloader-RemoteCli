using JDownloader.Cli.Commands.Extraction;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class CliApplicationExtractionRegistration
{
    public static void Register(IConfigurator config)
    {
        config.AddBranch("extraction", extraction =>
        {
            extraction.SetDescription("Inspect and control archive extraction.");
            extraction.AddCommand<ExtractionQueueCommand>("queue").WithDescription("Show extraction queue.");
            extraction.AddCommand<ExtractionInfoCommand>("info").WithDescription("Show extraction controller info.");
            extraction.AddCommand<ExtractionStartCommand>("start").WithDescription("Start extraction.");
            extraction.AddCommand<ExtractionCancelCommand>("cancel").WithDescription("Cancel extraction.");
            extraction.AddCommand<ExtractionAddPasswordCommand>("add-password").WithDescription("Add an extraction password.");
            extraction.AddBranch("settings", settings =>
            {
                settings.SetDescription("Inspect and update extraction settings.");
                settings.AddCommand<ExtractionSettingsGetCommand>("get").WithDescription("Get extraction settings.");
                settings.AddCommand<ExtractionSettingsSetCommand>("set").WithDescription("Update extraction settings.");
            });
        });
    }
}
