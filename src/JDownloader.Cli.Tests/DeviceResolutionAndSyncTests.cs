using System.Text.Json;
using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Tests.Support;
using JDownloader.Cli.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace JDownloader.Cli.Tests;

public sealed class DeviceResolutionAndSyncTests
{
    [Fact]
    public async Task DeviceCatalogPersistsKnownDevicesAndSetsSingleDefault()
    {
        var store = new InMemoryProfileStore
        {
            Config = new Jd2Config
            {
                Profiles =
                {
                    ["main"] = new ProfileRecord { AccountEmail = "user@example.com" },
                },
            },
        };
        var relayClient = new RecordingRelayClient
        {
            Devices = [new("dev-1", "Box", null, null)],
        };
        var catalog = new DeviceCatalog(relayClient, store);

        var devices = await catalog.SyncAsync("main", "user@example.com", 30, persist: true, CancellationToken.None);

        var profile = store.Config.Profiles["main"];
        Assert.Equal("dev-1", Assert.Single(devices).Id);
        Assert.Equal("dev-1", profile.DefaultDeviceId);
        Assert.Equal("Box", profile.DefaultDeviceName);
        Assert.Equal("Box", Assert.Single(profile.KnownDevices).Name);
    }

    [Fact]
    public async Task DryRunDeviceResolutionUsesLiveCatalogWithoutPersistingCache()
    {
        var home = CreateTempPath();
        var configRoot = Path.Combine(home, "cfg");
        var configPath = Path.Combine(configRoot, "config.json");
        var env = new FakeCliEnvironment(home, new Dictionary<string, string> { ["JD2_CONFIG"] = configRoot });

        await CliTestHarness.WriteConfigAsync(configPath, new Jd2Config
        {
            DefaultProfile = "main",
            Profiles =
            {
                ["main"] = new ProfileRecord
                {
                    AccountEmail = "user@example.com",
                    DefaultDeviceName = "Live Box",
                },
            },
        });

        var relayClient = new RecordingRelayClient
        {
            Devices = [new("dev-2", "Live Box", null, null)],
        };

        var result = await CliTestHarness.RunAsync(
            env,
            ["downloads", "status", "--dry-run", "--json"],
            configureServices: services => services.AddSingleton<IMyJdRelayClient>(relayClient));

        Assert.Equal(0, result.ExitCode);
        using var document = CliTestHarness.ParseJson(result.StdOut);
        Assert.Equal("dev-2", document.RootElement.GetProperty("data").GetProperty("device").GetProperty("id").GetString());
        var config = await ReadConfigAsync(configPath);
        Assert.Empty(config.Profiles["main"].KnownDevices);
    }

    [Fact]
    public async Task DeviceUsePersistsLiveDeviceSelection()
    {
        var home = CreateTempPath();
        var configRoot = Path.Combine(home, "cfg");
        var configPath = Path.Combine(configRoot, "config.json");
        var env = new FakeCliEnvironment(home, new Dictionary<string, string> { ["JD2_CONFIG"] = configRoot });

        await CliTestHarness.WriteConfigAsync(configPath, new Jd2Config
        {
            DefaultProfile = "main",
            Profiles =
            {
                ["main"] = new ProfileRecord
                {
                    AccountEmail = "user@example.com",
                },
            },
        });

        var relayClient = new RecordingRelayClient
        {
            Devices =
            [
                new("dev-2", "Live Box", null, null),
                new("dev-1", "Archive Box", null, null),
            ],
        };

        var result = await CliTestHarness.RunAsync(
            env,
            ["device", "use", "--device", "Live Box"],
            configureServices: services => services.AddSingleton<IMyJdRelayClient>(relayClient));

        Assert.Equal(0, result.ExitCode);
        var config = await ReadConfigAsync(configPath);
        var profile = config.Profiles["main"];
        Assert.Equal("dev-2", profile.DefaultDeviceId);
        Assert.Equal("Live Box", profile.DefaultDeviceName);
        Assert.Equal(2, profile.KnownDevices.Count);
    }

    [Fact]
    public async Task DeviceListQuietUsesCachedDevicesWithoutWarningNoise()
    {
        var home = CreateTempPath();
        var configRoot = Path.Combine(home, "cfg");
        var env = new FakeCliEnvironment(home, new Dictionary<string, string> { ["JD2_CONFIG"] = configRoot });

        await CliTestHarness.WriteConfigAsync(Path.Combine(configRoot, "config.json"), new Jd2Config
        {
            DefaultProfile = "main",
            Profiles =
            {
                ["main"] = new ProfileRecord
                {
                    AccountEmail = "user@example.com",
                    KnownDevices = [new() { Id = "dev-1", Name = "Cached Box", SeenAtUtc = DateTimeOffset.UtcNow }],
                },
            },
        });

        var result = await CliTestHarness.RunAsync(
            env,
            ["device", "list", "--quiet"],
            configureServices: services => services.AddSingleton<IDeviceCatalog>(new StubDeviceCatalog
            {
                SyncAsyncFunc = (_, _, _, _, _) => throw CliException.Transport("sync failed"),
            }));

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Cached Box (dev-1)", result.StdOut);
        Assert.True(string.IsNullOrWhiteSpace(result.StdErr));
    }

    private static async Task<Jd2Config> ReadConfigAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        return (await JsonSerializer.DeserializeAsync<Jd2Config>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        }))!;
    }

    private static string CreateTempPath()
    {
        var path = Path.Combine(Path.GetTempPath(), "jd2-cli-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
