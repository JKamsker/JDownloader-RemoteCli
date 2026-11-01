using System.Text.Json;
using JDownloader.Cli.Config;
using JDownloader.Cli.Tests.Support;

namespace JDownloader.Cli.Tests;

public sealed class ConnectionCommandTests
{
    [Fact]
    public async Task Add_DirectLocalhost_PersistsEndpointWithoutAccount()
    {
        var env = CreateEnv(out var configPath);

        var result = await CliTestHarness.RunAsync(env, ["connection", "add", "local", "--mode", "direct", "--host", "127.0.0.1", "--port", "3128", "--json"]);

        Assert.Equal(0, result.ExitCode);
        var config = ReadConfig(configPath);
        var profile = config.Profiles["local"];
        Assert.Equal("direct", profile.Mode);
        Assert.Equal("127.0.0.1", profile.Host);
        Assert.Equal(3128, profile.Port);
        Assert.Null(profile.AccountEmail);
        Assert.Null(profile.AllowRemote);
        Assert.Equal("local", config.DefaultProfile);
    }

    [Fact]
    public async Task Add_DirectDefaultsToLoopbackAndStandardPort()
    {
        var env = CreateEnv(out var configPath);

        var result = await CliTestHarness.RunAsync(env, ["connection", "add", "local", "--mode", "direct"]);

        Assert.Equal(0, result.ExitCode);
        var profile = ReadConfig(configPath).Profiles["local"];
        Assert.Equal("127.0.0.1", profile.Host);
        Assert.Equal(3128, profile.Port);
    }

    [Fact]
    public async Task Add_RemoteHostWithoutAllowRemote_IsRejected()
    {
        var env = CreateEnv(out var configPath);

        var result = await CliTestHarness.RunAsync(env, ["connection", "add", "nas", "--mode", "direct", "--host", "192.168.1.20", "--json"]);

        Assert.Equal(2, result.ExitCode);
        Assert.False(File.Exists(configPath));
        Assert.Contains("--allow-remote", result.StdErr + result.StdOut, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Add_DotLocalhostName_IsNotTreatedAsLoopback()
    {
        var env = CreateEnv(out _);

        var result = await CliTestHarness.RunAsync(env, ["connection", "add", "x", "--mode", "direct", "--host", "evil.localhost"]);

        Assert.Equal(2, result.ExitCode);
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("LOCALHOST")]
    [InlineData("127.0.0.2")]
    [InlineData("::1")]
    public async Task Add_LoopbackForms_AreAcceptedWithoutAllowRemote(string host)
    {
        var env = CreateEnv(out _);

        var result = await CliTestHarness.RunAsync(env, ["connection", "add", "x", "--mode", "direct", "--host", host]);

        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task Add_RemoteHostWithAllowRemote_SavesAndWarns()
    {
        var env = CreateEnv(out var configPath);

        var result = await CliTestHarness.RunAsync(env, ["connection", "add", "nas", "--mode", "direct", "--host", "192.168.1.20", "--allow-remote", "--json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.True(ReadConfig(configPath).Profiles["nas"].AllowRemote);
        using var document = CliTestHarness.ParseJson(result.StdOut);
        var warnings = document.RootElement.GetProperty("meta").GetProperty("warnings");
        Assert.Contains("no authentication", warnings[0].GetString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("http://localhost")]
    [InlineData("localhost/path")]
    [InlineData("user@192.168.1.5")]
    [InlineData("localhost:3128")]
    [InlineData("bad host")]
    public async Task Add_HostWithSchemeOrPath_IsRejected(string host)
    {
        var env = CreateEnv(out _);

        var result = await CliTestHarness.RunAsync(env, ["connection", "add", "bad", "--mode", "direct", "--host", host]);

        Assert.Equal(2, result.ExitCode);
    }

    [Fact]
    public async Task Add_DirectOptionsInMyJdMode_AreRejected()
    {
        var env = CreateEnv(out _);

        var result = await CliTestHarness.RunAsync(env, ["connection", "add", "cloud", "--mode", "myjd", "--host", "localhost"]);

        Assert.Equal(2, result.ExitCode);
    }

    [Fact]
    public async Task Add_ExistingWithoutForce_Conflicts()
    {
        var env = CreateEnv(out _);
        await CliTestHarness.RunAsync(env, ["connection", "add", "local", "--mode", "direct"]);

        var again = await CliTestHarness.RunAsync(env, ["connection", "add", "local", "--mode", "direct", "--port", "4000"]);
        var forced = await CliTestHarness.RunAsync(env, ["connection", "add", "local", "--mode", "direct", "--port", "4000", "--force"]);

        Assert.Equal(6, again.ExitCode);
        Assert.Equal(0, forced.ExitCode);
    }

    [Fact]
    public async Task UseAndList_ShowModeAndDefault()
    {
        var env = CreateEnv(out _);
        await CliTestHarness.RunAsync(env, ["connection", "add", "cloud"]);
        await CliTestHarness.RunAsync(env, ["connection", "add", "local", "--mode", "direct"]);
        var use = await CliTestHarness.RunAsync(env, ["connection", "use", "local"]);

        var list = await CliTestHarness.RunAsync(env, ["connection", "list", "--json"]);

        Assert.Equal(0, use.ExitCode);
        using var document = CliTestHarness.ParseJson(list.StdOut);
        var items = document.RootElement.GetProperty("data").EnumerateArray().ToDictionary(item => item.GetProperty("name").GetString()!);
        Assert.Equal("myjd", items["cloud"].GetProperty("mode").GetString());
        Assert.Equal("direct", items["local"].GetProperty("mode").GetString());
        Assert.True(items["local"].GetProperty("isDefault").GetBoolean());
        Assert.Equal("http://127.0.0.1:3128", items["local"].GetProperty("endpoint").GetString());
    }

    [Fact]
    public async Task DirectProfile_RejectsMyJdOnlyCommandsExplicitly()
    {
        var env = CreateEnv(out _);
        await CliTestHarness.RunAsync(env, ["connection", "add", "local", "--mode", "direct"]);

        var deviceList = await CliTestHarness.RunAsync(env, ["device", "list"]);
        var deviceUse = await CliTestHarness.RunAsync(env, ["device", "use", "--device", "x"]);
        var login = await CliTestHarness.RunAsync(env, ["auth", "login", "--email", "a@b.c", "--password-stdin"], input: "pw");
        var deviceFlag = await CliTestHarness.RunAsync(env, ["downloads", "status", "--device", "x"]);

        Assert.Equal(2, deviceList.ExitCode);
        Assert.Equal(2, deviceUse.ExitCode);
        Assert.Equal(2, login.ExitCode);
        Assert.Equal(2, deviceFlag.ExitCode);
    }

    [Fact]
    public async Task HandEditedRemoteProfileWithoutOptIn_IsRefusedAtRuntime()
    {
        var env = CreateEnv(out var configPath);
        var config = new Jd2Config { DefaultProfile = "nas" };
        config.Profiles["nas"] = new ProfileRecord { Mode = "direct", Host = "10.0.0.5", Port = 3128 };
        await CliTestHarness.WriteConfigAsync(configPath, config);

        var result = await CliTestHarness.RunAsync(env, ["downloads", "status", "--json"]);

        Assert.Equal(2, result.ExitCode);
        Assert.Contains("remote access was not explicitly allowed", result.StdErr + result.StdOut, StringComparison.Ordinal);
    }

    private static FakeCliEnvironment CreateEnv(out string configPath)
    {
        var root = Path.Combine(Path.GetTempPath(), "jd2-cli-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var env = new FakeCliEnvironment(root);
        configPath = new CliPathProvider(env).GetConfigFilePath();
        return env;
    }

    private static Jd2Config ReadConfig(string path)
    {
        return JsonSerializer.Deserialize<Jd2Config>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
    }
}
