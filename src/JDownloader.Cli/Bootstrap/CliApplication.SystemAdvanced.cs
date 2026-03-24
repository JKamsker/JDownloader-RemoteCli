using JDownloader.Cli.Commands.Advanced;
using JDownloader.Cli.Commands.System;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class CliApplicationSystemAdvancedRegistration
{
    public static void Register(IConfigurator config)
    {
        RegisterSystem(config);
        RegisterAdvanced(config);
    }

    private static void RegisterSystem(IConfigurator config)
    {
        config.AddBranch("system", system =>
        {
            system.SetDescription("JDownloader, OS, and update operations.");
            system.AddCommand<SystemInfoCommand>("info").WithDescription("Show JDownloader system info.");
            system.AddCommand<SystemStorageCommand>("storage").WithDescription("Show storage info.");
            system.AddCommand<SystemReconnectCommand>("reconnect").WithDescription("Trigger reconnect.");
            system.AddBranch("jd", jd =>
            {
                jd.SetDescription("JDownloader lifecycle operations.");
                jd.AddCommand<SystemJdVersionCommand>("version").WithDescription("Show JDownloader version.");
                jd.AddCommand<SystemJdRevisionCommand>("revision").WithDescription("Show JDownloader core revision.");
                jd.AddCommand<SystemJdUptimeCommand>("uptime").WithDescription("Show JDownloader uptime.");
                jd.AddCommand<SystemJdRefreshPluginsCommand>("refresh-plugins").WithDescription("Refresh plugins.");
                jd.AddCommand<SystemJdRestartCommand>("restart").WithDescription("Restart JDownloader.");
                jd.AddCommand<SystemJdExitCommand>("exit").WithDescription("Exit JDownloader.");
            });
            system.AddBranch("os", os =>
            {
                os.SetDescription("Operating system power operations.");
                os.AddCommand<SystemOsShutdownCommand>("shutdown").WithDescription("Shutdown the operating system.");
                os.AddCommand<SystemOsHibernateCommand>("hibernate").WithDescription("Hibernate the operating system.");
                os.AddCommand<SystemOsStandbyCommand>("standby").WithDescription("Put the operating system into standby.");
            });
            system.AddBranch("update", update =>
            {
                update.SetDescription("Update lifecycle operations.");
                update.AddCommand<SystemUpdateCheckCommand>("check").WithDescription("Check whether updates are available.");
                update.AddCommand<SystemUpdateRunCommand>("run").WithDescription("Run update check.");
                update.AddCommand<SystemUpdateRestartCommand>("restart").WithDescription("Restart and apply updates.");
            });
            system.AddCommand<SystemToggleCommand>("toggle").WithDescription("Toggle common JDownloader state flags.");
        });
    }

    private static void RegisterAdvanced(IConfigurator config)
    {
        config.AddBranch("advanced", advanced =>
        {
            advanced.SetDescription("Expert-only escape hatches and raw access.");
            advanced.AddBranch("content", content =>
            {
                content.SetDescription("Binary icons and content endpoints.");
                content.AddCommand<AdvancedContentIconCommand>("icon").WithDescription("Fetch an icon by key (binary).");
                content.AddCommand<AdvancedContentFavIconCommand>("favicon").WithDescription("Fetch a favicon by hoster name (binary).");
                content.AddCommand<AdvancedContentFileIconCommand>("file-icon").WithDescription("Fetch a file icon by extension (binary).");
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
                ingest.AddCommand<AdvancedIngestCnlCommand>("cnl").WithDescription("Ingest a Click'n'Load (CNL) payload.");
            });
            advanced.AddBranch("raw", raw =>
            {
                raw.SetDescription("Raw endpoint escape hatch.");
                raw.AddCommand<AdvancedRawRequestCommand>("request").WithDescription("Send a raw My.JDownloader endpoint request.");
            });
        });
    }
}
