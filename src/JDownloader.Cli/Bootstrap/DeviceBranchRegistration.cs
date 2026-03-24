using JDownloader.Cli.Commands.Device;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class DeviceBranchRegistration
{
    public static void RegisterDeviceCommands(this IConfigurator config)
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
}
