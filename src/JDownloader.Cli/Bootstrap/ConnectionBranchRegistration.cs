using JDownloader.Cli.Commands.Auth;
using JDownloader.Cli.Commands.Connection;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class ConnectionBranchRegistration
{
    public static void RegisterConnectionCommands(this IConfigurator config)
    {
        config.AddBranch("connection", connection =>
        {
            connection.SetDescription("Manage saved connections: direct (local JDownloader API) or My.JDownloader.");
            connection.AddCommand<ConnectionListCommand>("list").WithDescription("List saved connections with their mode and endpoint.");
            connection.AddCommand<ConnectionGetCommand>("get").WithDescription("Show a saved connection.");
            connection.AddCommand<ConnectionAddCommand>("add").WithDescription("Create or update a connection (--mode direct|myjd).");
            connection.AddCommand<ConnectionTestCommand>("test").WithDescription("Probe the resolved connection (ping, version, download state) and report latency.");
            connection.AddCommand<UseProfileCommand>("use").WithDescription("Set the default connection.");
            connection.AddCommand<RemoveProfileCommand>("remove").WithDescription("Remove a saved connection.");
        });
    }
}
