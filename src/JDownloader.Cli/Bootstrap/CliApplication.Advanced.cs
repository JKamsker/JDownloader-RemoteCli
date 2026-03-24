using JDownloader.Cli.Commands.Advanced;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class CliApplicationAdvancedRegistration
{
    public static void Register(IConfigurator config)
    {
        config.AddBranch("advanced", advanced =>
        {
            advanced.SetDescription("Expert-only escape hatches and raw access.");
            advanced.AddBranch("content", content =>
            {
                content.SetDescription("Binary icons and content endpoints.");
                content.AddCommand<AdvancedContentIconCommand>("icon").WithDescription("Fetch an icon by key (binary).");
                content.AddCommand<AdvancedContentFavIconCommand>("favicon").WithDescription("Fetch a favicon by hoster name (binary).");
                content.AddCommand<AdvancedContentFileIconCommand>("file-icon").WithDescription("Fetch a file icon by filename (binary).");
                content.AddCommand<AdvancedContentDescribeIconCommand>("describe").WithDescription("Describe an icon key.");
            });
            advanced.AddBranch("dialogs", dialogs =>
            {
                dialogs.SetDescription("Inspect and answer advanced dialogs.");
                dialogs.AddCommand<AdvancedDialogsListCommand>("list").WithDescription("List open dialogs.");
                dialogs.AddCommand<AdvancedDialogsGetCommand>("get").WithDescription("Get a dialog by id.");
                dialogs.AddCommand<AdvancedDialogsAnswerCommand>("answer").WithDescription("Answer a dialog.");
                dialogs.AddCommand<AdvancedDialogsTypeInfoCommand>("type-info").WithDescription("Inspect a dialog type.");
            });
            advanced.AddBranch("ingest", ingest =>
            {
                ingest.SetDescription("Ingest helper endpoints.");
                ingest.AddCommand<AdvancedIngestCnlCommand>("cnl").WithDescription("Ingest a Click'n'Load payload via /flash/addcnl.");
            });
            advanced.AddBranch("raw", raw =>
            {
                raw.SetDescription("Raw endpoint escape hatch.");
                raw.AddCommand<AdvancedRawRequestCommand>("request").WithDescription("Send a raw My.JDownloader endpoint request.");
            });
        });
    }
}
