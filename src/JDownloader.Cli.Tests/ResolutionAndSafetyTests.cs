using JDownloader.Cli.Config;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Tests.Support;

namespace JDownloader.Cli.Tests;

public sealed class ResolutionAndSafetyTests
{
    [Fact]
    public async Task ResolutionUsesFlagsThenEnvThenConfigAndAmbiguousNamesFail()
    {
        var home = CreateTempPath();
        var configRoot = Path.Combine(home, "cfg");
        var env = new FakeCliEnvironment(home, new Dictionary<string, string>
        {
            ["JD2_CONFIG"] = configRoot,
            ["JD2_PROFILE"] = "env",
        });

        await CliTestHarness.WriteConfigAsync(Path.Combine(configRoot, "config.json"), new Jd2Config
        {
            DefaultProfile = "cfg",
            Profiles =
            {
                ["cfg"] = new ProfileRecord
                {
                    DefaultDeviceId = "cfg-1",
                    DefaultDeviceName = "CfgBox",
                    KnownDevices = [new() { Id = "cfg-1", Name = "CfgBox", SeenAtUtc = DateTimeOffset.UtcNow }],
                },
                ["env"] = new ProfileRecord
                {
                    DefaultDeviceId = "env-1",
                    DefaultDeviceName = "EnvBox",
                    KnownDevices = [new() { Id = "env-1", Name = "EnvBox", SeenAtUtc = DateTimeOffset.UtcNow }],
                },
                ["flag"] = new ProfileRecord
                {
                    DefaultDeviceId = "flag-1",
                    DefaultDeviceName = "FlagBox",
                    KnownDevices =
                    [
                        new() { Id = "flag-1", Name = "FlagBox", SeenAtUtc = DateTimeOffset.UtcNow },
                        new() { Id = "dup-1", Name = "dup", SeenAtUtc = DateTimeOffset.UtcNow },
                        new() { Id = "dup-2", Name = "Dup", SeenAtUtc = DateTimeOffset.UtcNow },
                    ],
                },
            },
        });

        var envResolved = await CliTestHarness.RunAsync(env, ["doctor", "--json"]);
        Assert.Equal(0, envResolved.ExitCode);
        Assert.Contains("\"resolvedProfile\": \"env\"", envResolved.StdOut);

        var flagResolved = await CliTestHarness.RunAsync(env, ["device", "get", "--json", "--profile", "flag"]);
        Assert.Equal(0, flagResolved.ExitCode);
        Assert.Contains("\"profile\": \"flag\"", flagResolved.StdOut);

        var ambiguous = await CliTestHarness.RunAsync(env, ["device", "get", "--json", "--profile", "flag", "--device", "DuP"]);
        Assert.Equal(2, ambiguous.ExitCode);
        Assert.Contains("\"kind\": \"usage\"", ambiguous.StdOut);
    }

    [Fact]
    public async Task DestructiveCommandsRequireYesOrDryRunAndDryRunUsesJsonEnvelope()
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
                    DefaultDeviceId = "dev-1",
                    DefaultDeviceName = "Box",
                    KnownDevices = [new() { Id = "dev-1", Name = "Box", SeenAtUtc = DateTimeOffset.UtcNow }],
                },
            },
        });

        var refused = await CliTestHarness.RunAsync(env, ["downloads", "links", "remove", "--quiet", "--json"]);
        Assert.Equal(2, refused.ExitCode);
        Assert.Contains("\"kind\": \"usage\"", refused.StdOut);

        var preview = await CliTestHarness.RunAsync(env, ["downloads", "links", "remove", "--dry-run", "--json"]);
        Assert.Equal(0, preview.ExitCode);
        Assert.Contains("\"ok\": true", preview.StdOut);
        Assert.Contains("\"action\": \"dry-run\"", preview.StdOut);
    }

    public static IEnumerable<object[]> DryRunJsonSecretCommands =>
    [
        [new[] { "accounts", "add", "--hoster", "ddownload.com", "--username", "alice", "--dry-run", "--json" }],
        [new[] { "accounts", "update", "--account-id", "123", "--username", "alice", "--dry-run", "--json" }],
        [new[] { "accounts", "basic-auth", "add", "--type", "http", "--hostmask", "example.com", "--username", "bob", "--dry-run", "--json" }],
        [new[] { "accounts", "basic-auth", "update", "--basic-auth-id", "123", "--type", "http", "--hostmask", "example.com", "--username", "bob", "--dry-run", "--json" }],
        [new[] { "extraction", "add-password", "--dry-run", "--json" }],
        [new[] { "advanced", "ingest", "cnl", "--url", "https://example.com", "--password", "supersecret", "--dry-run", "--json" }],
    ];

    [Theory]
    [MemberData(nameof(DryRunJsonSecretCommands))]
    public async Task DryRunJsonSecretCommandsExitZeroAndEmitSingleEnvelope(string[] args)
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
                    DefaultDeviceId = "dev-1",
                    DefaultDeviceName = "Box",
                    KnownDevices = [new() { Id = "dev-1", Name = "Box", SeenAtUtc = DateTimeOffset.UtcNow }],
                },
            },
        });

        var result = await CliTestHarness.RunAsync(env, args);
        Assert.Equal(0, result.ExitCode);
        Assert.True(string.IsNullOrWhiteSpace(result.StdErr));

        using var document = CliTestHarness.ParseJson(result.StdOut);
        Assert.True(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("dry-run", document.RootElement.GetProperty("data").GetProperty("action").GetString());

        if (document.RootElement.GetProperty("data").TryGetProperty("query", out var query)
            && query.ValueKind == System.Text.Json.JsonValueKind.Object
            && query.TryGetProperty("password", out var password))
        {
            Assert.Equal(SecretInput.Redacted, password.GetString());
        }
    }

    private static string CreateTempPath()
    {
        var path = Path.Combine(Path.GetTempPath(), "jd2-cli-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
