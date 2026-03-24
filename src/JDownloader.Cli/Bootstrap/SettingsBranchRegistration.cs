using JDownloader.Cli.Commands.Settings;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class SettingsBranchRegistration
{
    public static void RegisterSettingsCommands(this IConfigurator config)
    {
        config.AddBranch("settings", settings =>
        {
            settings.SetDescription("Inspect config, plugins, and extensions.");
            settings.AddBranch("config", configBranch =>
            {
                configBranch.SetDescription("Read and update config values.");
                configBranch.AddCommand<SettingsConfigListCommand>("list").WithDescription("List config entries for an interface.");
                configBranch.AddCommand<SettingsConfigGetCommand>("get").WithDescription("Get a config entry.");
                configBranch.AddCommand<SettingsConfigSetCommand>("set").WithDescription("Set a config entry.");
                configBranch.AddCommand<SettingsConfigResetCommand>("reset").WithDescription("Reset a config entry to default.");
            });
            settings.AddBranch("plugins", plugins =>
            {
                plugins.SetDescription("Inspect and resolve plugin config values.");
                plugins.AddCommand<SettingsPluginsListCommand>("list").WithDescription("List plugins.");
                plugins.AddCommand<SettingsPluginsGetCommand>("get").WithDescription("Get a plugin config entry.");
            });
            settings.AddBranch("extensions", extensions =>
            {
                extensions.SetDescription("Inspect and manage extensions.");
                extensions.AddCommand<SettingsExtensionsListCommand>("list").WithDescription("List extensions.");
                extensions.AddCommand<SettingsExtensionsGetCommand>("get").WithDescription("Get an extension by id or name.");
                extensions.AddCommand<SettingsExtensionsInstallCommand>("install").WithDescription("Install an extension by id.");
                extensions.AddCommand<SettingsExtensionsEnableCommand>("enable").WithDescription("Enable an extension by id or classname.");
                extensions.AddCommand<SettingsExtensionsDisableCommand>("disable").WithDescription("Disable an extension by id or classname.");
            });
        });
    }
}
