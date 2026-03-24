using JDownloader.Cli.Auth;
using JDownloader.Cli.Commands.Accounts;
using JDownloader.Cli.Commands.Advanced;
using JDownloader.Cli.Commands.Auth;
using JDownloader.Cli.Commands.Captcha;
using JDownloader.Cli.Commands.Device;
using JDownloader.Cli.Commands.Doctor;
using JDownloader.Cli.Commands.Downloads;
using JDownloader.Cli.Commands.Events;
using JDownloader.Cli.Commands.Extraction;
using JDownloader.Cli.Commands.Grabber;
using JDownloader.Cli.Commands.Settings;
using JDownloader.Cli.Commands.System;
using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System.Reflection;

namespace JDownloader.Cli.Bootstrap;

public static class CliApplication
{
    public static CommandApp Create(ICliEnvironment? environment = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICliEnvironment>(environment ?? new SystemCliEnvironment());
        services.AddSingleton<CliPathProvider>();
        services.AddSingleton<IProfileStore, FileProfileStore>();
        services.AddSingleton<IKeyFileProvider, FileKeyFileProvider>();
        services.AddSingleton<ICredentialProtector, AesCredentialProtector>();
        services.AddSingleton<IProfileResolver, ProfileResolver>();
        services.AddSingleton<IOutputRenderer, OutputRenderer>();
        services.AddSingleton<IDiagnosticLogger, DiagnosticLogger>();
        services.AddSingleton<IConfirmationGuard, ConfirmationGuard>();
        services.AddSingleton<IMyJdAuthService, MyJdAuthService>();
        services.AddSingleton<IRequestIdProvider, TimestampRequestIdProvider>();
        services.AddSingleton<IMyJdRelayClient, MyJdRelayClient>();
        services.AddSingleton<IDeviceCatalog, DeviceCatalog>();
        services.AddSingleton<IMyJdTransport, LiveMyJdTransport>();

        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(config =>
        {
            config.SetApplicationName("jdr");
            config.SetApplicationVersion(GetVersion());

            RegisterAuth(config);
            RegisterDevice(config);
            RegisterDownloads(config);
            RegisterGrabber(config);
            RegisterAccounts(config);
            RegisterExtraction(config);
            RegisterSettings(config);
            RegisterCaptcha(config);
            RegisterEvents(config);
            RegisterSystem(config);
            RegisterAdvanced(config);
            config.AddCommand<DoctorCommand>("doctor").WithDescription("Inspect config paths, resolution, and stored auth state.");
        });

        return app;
    }

    private static string GetVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? typeof(CliApplication).Assembly;
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
            return informational;

        return assembly.GetName().Version?.ToString() ?? "0.0.0";
    }

    private static void RegisterAuth(IConfigurator config)
    {
        config.AddBranch("auth", auth =>
        {
            auth.SetDescription("Authentication, identity, and saved profiles.");
            auth.AddCommand<LoginCommand>("login").WithDescription("Store encrypted auth material for a profile.");
            auth.AddCommand<LogoutCommand>("logout").WithDescription("Remove stored auth material for the resolved profile.");
            auth.AddCommand<AuthStatusCommand>("status").WithDescription("Show stored auth state for the resolved profile.");
            auth.AddCommand<WhoAmICommand>("whoami").WithDescription("Show the resolved profile and stored account.");
            auth.AddBranch("profiles", profiles =>
            {
                profiles.SetDescription("Manage saved CLI profiles.");
                profiles.AddCommand<ListProfilesCommand>("list").WithDescription("List saved profiles.");
                profiles.AddCommand<GetProfileCommand>("get").WithDescription("Show a saved profile.");
                profiles.AddCommand<AddProfileCommand>("add").WithDescription("Create a new profile.");
                profiles.AddCommand<RenameProfileCommand>("rename").WithDescription("Rename an existing profile.");
                profiles.AddCommand<RemoveProfileCommand>("remove").WithDescription("Remove a profile and its device defaults.");
                profiles.AddCommand<UseProfileCommand>("use").WithDescription("Set the default profile.");
            });
        });
    }

    private static void RegisterDevice(IConfigurator config)
    {
        config.AddBranch("device", device =>
        {
            device.SetDescription("Resolve, inspect, and select JDownloader devices.");
            device.AddCommand<ListDevicesCommand>("list").WithDescription("List devices visible to the current account.");
            device.AddCommand<GetDeviceCommand>("get").WithDescription("Show the currently resolved device.");
            device.AddCommand<UseDeviceCommand>("use").WithDescription("Set the default device for a profile.");
            device.AddCommand<DevicePingCommand>("ping").WithDescription("Ping the resolved device (0-arg endpoint).");
            device.AddCommand<DeviceDirectInfoCommand>("direct-info").WithDescription("Show direct connection info for the resolved device.");
        });
    }

    private static void RegisterDownloads(IConfigurator config)
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
                links.AddCommand<DownloadsLinksListCommand>("list").WithDescription("List download links.");
                links.AddCommand<DownloadsLinksRemoveCommand>("remove").WithDescription("Remove download links by id.");
            });
            downloads.AddBranch("packages", packages =>
            {
                packages.AddCommand<DownloadsPackagesListCommand>("list").WithDescription("List download packages.");
                packages.AddCommand<DownloadsPackagesRemoveCommand>("remove").WithDescription("Remove download packages by id.");
            });
            downloads.AddBranch("stopmark", stopmark =>
            {
                stopmark.AddCommand<DownloadsStopmarkGetCommand>("get").WithDescription("Get the current stopmark.");
                stopmark.AddCommand<DownloadsStopmarkSetCommand>("set").WithDescription("Set the stopmark (requires link + package id).");
                stopmark.AddCommand<DownloadsStopmarkClearCommand>("clear").WithDescription("Clear the stopmark.");
            });
        });
    }

    private static void RegisterGrabber(IConfigurator config)
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
                links.AddCommand<GrabberLinksListCommand>("list").WithDescription("List linkgrabber links.");
                links.AddCommand<GrabberLinksRemoveCommand>("remove").WithDescription("Remove linkgrabber links by id.");
            });
            grabber.AddBranch("packages", packages =>
            {
                packages.AddCommand<GrabberPackagesListCommand>("list").WithDescription("List linkgrabber packages.");
                packages.AddCommand<GrabberPackagesRemoveCommand>("remove").WithDescription("Remove linkgrabber packages by id.");
            });
            grabber.AddBranch("jobs", jobs =>
            {
                jobs.AddCommand<GrabberJobsListCommand>("list").WithDescription("List crawler jobs.");
                jobs.AddCommand<GrabberJobsGetCommand>("get").WithDescription("Get crawler jobs by id.");
            });
            grabber.AddBranch("variants", variants =>
            {
                variants.AddCommand<GrabberVariantsListCommand>("list").WithDescription("List variants for the current selection.");
                variants.AddCommand<GrabberVariantsSetCommand>("set").WithDescription("Select a variant for a linkgrabber link.");
            });
        });
    }

    private static void RegisterAccounts(IConfigurator config)
    {
        config.AddBranch("accounts", accounts =>
        {
            accounts.SetDescription("Manage premium accounts and basic-auth entries.");
            accounts.AddCommand<AccountsListCommand>("list").WithDescription("List accounts.");
            accounts.AddCommand<AccountsGetCommand>("get").WithDescription("Legacy: resolve a premium hoster URL (prefer 'accounts hosters url').");
            accounts.AddCommand<AccountsAddCommand>("add").WithDescription("Add an account.");
            accounts.AddCommand<AccountsUpdateCommand>("update").WithDescription("Update username/password for an account id.");
            accounts.AddCommand<AccountsEnableCommand>("enable").WithDescription("Enable accounts by id.");
            accounts.AddCommand<AccountsDisableCommand>("disable").WithDescription("Disable accounts by id.");
            accounts.AddCommand<AccountsRemoveCommand>("remove").WithDescription("Remove accounts by id.");
            accounts.AddCommand<AccountsRefreshCommand>("refresh").WithDescription("Refresh accounts by id.");
            accounts.AddBranch("hosters", hosters =>
            {
                hosters.AddCommand<AccountsHostersListCommand>("list").WithDescription("List premium hosters.");
                hosters.AddCommand<AccountsHostersUrlCommand>("url").WithDescription("Resolve a premium hoster name to its account URL.");
                hosters.AddCommand<AccountsHostersUrlsCommand>("urls").WithDescription("List premium hoster URLs.");
            });
            accounts.AddBranch("basic-auth", basicAuth =>
            {
                basicAuth.AddCommand<AccountsBasicAuthListCommand>("list").WithDescription("List basic-auth entries.");
                basicAuth.AddCommand<AccountsBasicAuthAddCommand>("add").WithDescription("Add a basic-auth entry.");
                basicAuth.AddCommand<AccountsBasicAuthUpdateCommand>("update").WithDescription("Update a basic-auth entry.");
                basicAuth.AddCommand<AccountsBasicAuthRemoveCommand>("remove").WithDescription("Remove basic-auth entries by id.");
            });
        });
    }

    private static void RegisterExtraction(IConfigurator config)
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
                settings.AddCommand<ExtractionSettingsGetCommand>("get").WithDescription("Get extraction settings.");
                settings.AddCommand<ExtractionSettingsSetCommand>("set").WithDescription("Update extraction settings.");
            });
        });
    }

    private static void RegisterSettings(IConfigurator config)
    {
        config.AddBranch("settings", settings =>
        {
            settings.SetDescription("Inspect config, plugins, and extensions.");
            settings.AddBranch("config", configBranch =>
            {
                configBranch.AddCommand<SettingsConfigListCommand>("list").WithDescription("List config entries for an interface.");
                configBranch.AddCommand<SettingsConfigGetCommand>("get").WithDescription("Get a config entry.");
                configBranch.AddCommand<SettingsConfigSetCommand>("set").WithDescription("Set a config entry.");
                configBranch.AddCommand<SettingsConfigResetCommand>("reset").WithDescription("Reset a config entry to default.");
            });
            settings.AddBranch("plugins", plugins =>
            {
                plugins.AddCommand<SettingsPluginsListCommand>("list").WithDescription("List plugins.");
                plugins.AddCommand<SettingsPluginsGetCommand>("get").WithDescription("Get a plugin by classname.");
            });
            settings.AddBranch("extensions", extensions =>
            {
                extensions.AddCommand<SettingsExtensionsListCommand>("list").WithDescription("List extensions.");
                extensions.AddCommand<SettingsExtensionsGetCommand>("get").WithDescription("Get an extension by classname.");
                extensions.AddCommand<SettingsExtensionsInstallCommand>("install").WithDescription("Install an extension by id.");
                extensions.AddCommand<SettingsExtensionsEnableCommand>("enable").WithDescription("Enable an extension by classname.");
                extensions.AddCommand<SettingsExtensionsDisableCommand>("disable").WithDescription("Disable an extension by classname.");
            });
        });
    }

    private static void RegisterCaptcha(IConfigurator config)
    {
        config.AddBranch("captcha", captcha =>
        {
            captcha.SetDescription("Inspect and answer captcha jobs.");
            captcha.AddCommand<CaptchaListCommand>("list").WithDescription("List captcha jobs.");
            captcha.AddCommand<CaptchaGetCommand>("get").WithDescription("Get a captcha job.");
            captcha.AddCommand<CaptchaJobCommand>("job").WithDescription("Get captcha job details.");
            captcha.AddCommand<CaptchaSolveCommand>("solve").WithDescription("Submit a captcha answer.");
            captcha.AddCommand<CaptchaSkipCommand>("skip").WithDescription("Skip a captcha.");
            captcha.AddBranch("forward", forward =>
            {
                forward.AddCommand<CaptchaForwardCreateJobCommand>("create-job").WithDescription("Create a captcha forward job (RecaptchaV2).");
                forward.AddCommand<CaptchaForwardGetResultCommand>("get-result").WithDescription("Fetch a captcha forward result by job id.");
            });
        });
    }

    private static void RegisterEvents(IConfigurator config)
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
                jd.AddCommand<SystemJdVersionCommand>("version").WithDescription("Show JDownloader version.");
                jd.AddCommand<SystemJdRevisionCommand>("revision").WithDescription("Show JDownloader core revision.");
                jd.AddCommand<SystemJdUptimeCommand>("uptime").WithDescription("Show JDownloader uptime.");
                jd.AddCommand<SystemJdRefreshPluginsCommand>("refresh-plugins").WithDescription("Refresh plugins.");
                jd.AddCommand<SystemJdRestartCommand>("restart").WithDescription("Restart JDownloader.");
                jd.AddCommand<SystemJdExitCommand>("exit").WithDescription("Exit JDownloader.");
            });
            system.AddBranch("os", os =>
            {
                os.AddCommand<SystemOsShutdownCommand>("shutdown").WithDescription("Shutdown the operating system.");
                os.AddCommand<SystemOsHibernateCommand>("hibernate").WithDescription("Hibernate the operating system.");
                os.AddCommand<SystemOsStandbyCommand>("standby").WithDescription("Put the operating system into standby.");
            });
            system.AddBranch("update", update =>
            {
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
                content.AddCommand<AdvancedContentIconCommand>("icon").WithDescription("Fetch an icon by key (binary).");
                content.AddCommand<AdvancedContentFavIconCommand>("favicon").WithDescription("Fetch a favicon by URL (binary).");
                content.AddCommand<AdvancedContentFileIconCommand>("file-icon").WithDescription("Fetch a file icon by extension (binary).");
                content.AddCommand<AdvancedContentDescribeIconCommand>("describe").WithDescription("Describe an icon key.");
            });
            advanced.AddBranch("dialogs", dialogs =>
            {
                dialogs.AddCommand<AdvancedDialogsListCommand>("list").WithDescription("List open dialogs.");
                dialogs.AddCommand<AdvancedDialogsGetCommand>("get").WithDescription("Get a dialog by id.");
                dialogs.AddCommand<AdvancedDialogsAnswerCommand>("answer").WithDescription("Answer a dialog.");
                dialogs.AddCommand<AdvancedDialogsTypeInfoCommand>("type-info").WithDescription("Inspect a dialog type.");
            });
            advanced.AddBranch("ingest", ingest =>
            {
                ingest.AddCommand<AdvancedIngestCnlCommand>("cnl").WithDescription("Ingest a Click'n'Load (CNL) payload.");
            });
            advanced.AddBranch("raw", raw =>
            {
                raw.AddCommand<AdvancedRawRequestCommand>("request").WithDescription("Send a raw My.JDownloader endpoint request.");
            });
        });
    }
}
