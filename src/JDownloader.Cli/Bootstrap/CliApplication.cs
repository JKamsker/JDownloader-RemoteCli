using System.Reflection;
using JDownloader.Cli.Auth;
using JDownloader.Cli.Commands.Doctor;
using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

public static class CliApplication
{
    public static CommandApp Create(ICliEnvironment? environment = null, Action<IServiceCollection>? configureServices = null)
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
        configureServices?.Invoke(services);

        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(config =>
        {
            config.SetApplicationName("jdr");
            config.SetApplicationVersion(GetVersion());

            CliApplicationAuthRegistration.Register(config);
            CliApplicationDeviceRegistration.Register(config);
            CliApplicationDownloadsRegistration.Register(config);
            CliApplicationGrabberRegistration.Register(config);
            CliApplicationAccountsRegistration.Register(config);
            CliApplicationExtractionRegistration.Register(config);
            CliApplicationSettingsRegistration.Register(config);
            CliApplicationCaptchaRegistration.Register(config);
            CliApplicationEventsRegistration.Register(config);
            CliApplicationSystemRegistration.Register(config);
            CliApplicationAdvancedRegistration.Register(config);
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
}
