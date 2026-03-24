using JDownloader.Cli.Commands.System;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class CliApplicationSystemRegistration
{
    public static void Register(IConfigurator config)
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
}
