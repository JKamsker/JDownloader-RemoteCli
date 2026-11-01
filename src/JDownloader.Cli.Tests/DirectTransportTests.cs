using System.Net;
using System.Text.Json;
using JDownloader.Cli.Config;
using JDownloader.Cli.Tests.Support;

namespace JDownloader.Cli.Tests;

public sealed class DirectTransportTests
{
    [Fact]
    public async Task DownloadsStatus_PostsEmptyArrayToLocalEndpoint_AndUnwrapsData()
    {
        var handler = FakeDirectHandler.Json("""{"data":{"state":"RUNNING"},"rid":-1}""");
        var run = await RunAsync(handler, ["downloads", "status", "--json"]);

        Assert.Equal(0, run.Result.ExitCode);
        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("http://127.0.0.1:3128/downloadcontroller/getCurrentState", request.Uri.ToString());
        Assert.Equal("application/json", request.ContentType);
        Assert.Equal("{\"params\":[]}", request.Body);
        Assert.False(request.HasOrigin);
        using var document = CliTestHarness.ParseJson(run.Result.StdOut);
        Assert.Equal("RUNNING", document.RootElement.GetProperty("data").GetProperty("state").GetString());
        Assert.Empty(run.Relay.Invocations);
    }

    [Fact]
    public async Task ObjectParameters_AreSentAsRealJsonObjects_NotJsonStrings()
    {
        var handler = FakeDirectHandler.Json("""{"data":[],"rid":-1}""");
        var run = await RunAsync(handler, ["downloads", "links", "list", "--limit", "5", "--json"]);

        Assert.Equal(0, run.Result.ExitCode);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("/downloadsV2/queryLinks", request.Uri.AbsolutePath);
        using var body = JsonDocument.Parse(request.Body);
        var first = Assert.Single(body.RootElement.GetProperty("params").EnumerateArray());
        Assert.Equal(JsonValueKind.Object, first.ValueKind);
        Assert.Equal(5, first.GetProperty("maxResults").GetInt32());
    }

    [Fact]
    public async Task DevicePing_WorksLocally()
    {
        var handler = FakeDirectHandler.Json("""{"data":true,"rid":-1}""");
        var run = await RunAsync(handler, ["device", "ping", "--json"]);

        Assert.Equal(0, run.Result.ExitCode);
        Assert.Equal("/device/ping", Assert.Single(handler.Requests).Uri.AbsolutePath);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "404")]
    [InlineData(HttpStatusCode.BadRequest, "400")]
    [InlineData(HttpStatusCode.InternalServerError, "500")]
    [InlineData(HttpStatusCode.Forbidden, "403")]
    public async Task HttpErrors_BecomeTransportErrorsWithStatus(HttpStatusCode status, string expected)
    {
        var handler = FakeDirectHandler.Json("""{"src":"DEVICE","type":"BAD_PARAMETERS","data":null}""", status);
        var run = await RunAsync(handler, ["downloads", "status", "--json"]);

        Assert.Equal(8, run.Result.ExitCode);
        Assert.Contains(expected, run.Result.StdErr + run.Result.StdOut, StringComparison.Ordinal);
        Assert.Empty(run.Relay.Invocations);
    }

    [Fact]
    public async Task ConnectionRefused_NeverFallsBackToMyJd_AndExplainsHowToEnableApi()
    {
        var handler = FakeDirectHandler.Throwing(FakeDirectHandler.ConnectionRefused());
        var run = await RunAsync(handler, ["downloads", "status", "--json"]);

        Assert.Equal(8, run.Result.ExitCode);
        var output = run.Result.StdErr + run.Result.StdOut;
        Assert.Contains("Connection refused", output, StringComparison.Ordinal);
        Assert.Contains("deprecatedapienabled", output, StringComparison.Ordinal);
        Assert.Empty(run.Relay.Invocations);
    }

    [Fact]
    public async Task UserStringsThatLookLikeJson_StayStrings()
    {
        var handler = FakeDirectHandler.Json("""{"data":null}""");
        var root = Path.Combine(Path.GetTempPath(), "jd2-cli-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var env = new FakeCliEnvironment(root);
        var config = new Jd2Config { DefaultProfile = "p" };
        config.Profiles["p"] = new ProfileRecord { Mode = "direct" };
        await CliTestHarness.WriteConfigAsync(new CliPathProvider(env).GetConfigFilePath(), config);

        var result = await CliTestHarness.RunAsync(
            env,
            ["accounts", "add", "--hoster", "example.com", "--username", "user", "--password-stdin", "--yes", "--json"],
            input: "[1234]",
            configureServices: handler.Configure(new RecordingRelayClient()));

        Assert.Equal(0, result.ExitCode);
        using var body = JsonDocument.Parse(Assert.Single(handler.Requests).Body);
        Assert.Contains(body.RootElement.GetProperty("params").EnumerateArray(), item => item.ValueKind == JsonValueKind.String && item.GetString() == "[1234]");
    }

    [Fact]
    public async Task ConnectTimeout_IsReportedAsTransportError()
    {
        var handler = FakeDirectHandler.Throwing(new TaskCanceledException("connect", new TimeoutException()));
        var run = await RunAsync(handler, ["downloads", "status", "--json"]);

        Assert.Equal(8, run.Result.ExitCode);
        Assert.Contains("timed out", run.Result.StdErr + run.Result.StdOut, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SlowServer_TimesOutWithConfiguredTimeout()
    {
        var handler = new FakeDirectHandler(async (_, token) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(30), token);
            return FakeDirectHandler.JsonResponse("{}");
        });
        var run = await RunAsync(handler, ["downloads", "status", "--timeout", "1", "--json"]);

        Assert.Equal(8, run.Result.ExitCode);
        Assert.Contains("Timed out after 1s", run.Result.StdErr + run.Result.StdOut, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NonJsonAnswer_IsReportedAsNotJDownloader()
    {
        var handler = new FakeDirectHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html>nginx</html>", System.Text.Encoding.UTF8, "text/html"),
        }));
        var run = await RunAsync(handler, ["downloads", "status", "--json"]);

        Assert.Equal(8, run.Result.ExitCode);
        Assert.Contains("does not look like JDownloader", run.Result.StdErr + run.Result.StdOut, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BinaryResponses_AreReturnedAsBytes()
    {
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        var handler = new FakeDirectHandler((_, _) =>
        {
            var content = new ByteArrayContent(png);
            content.Headers.ContentType = new("image/png");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        });
        var target = Path.Combine(Path.GetTempPath(), "jd2-cli-tests", Guid.NewGuid().ToString("N"), "icon.png");
        var run = await RunAsync(handler, ["advanced", "raw", "request", "/contentV2/getFavIcon", "--query-json", "[\"example.com\"]", "--output-file", target, "--json"]);

        Assert.Equal(0, run.Result.ExitCode);
        Assert.Equal(png, await File.ReadAllBytesAsync(target));
    }

    [Theory]
    [InlineData("/my/listdevices")]
    [InlineData("/session/handshake")]
    public async Task CloudOnlyEndpoints_AreRejectedExplicitlyWithoutAnyRequest(string endpoint)
    {
        var handler = FakeDirectHandler.Json("{}");
        var run = await RunAsync(handler, ["advanced", "raw", "request", endpoint, "--json"]);

        Assert.Equal(2, run.Result.ExitCode);
        Assert.Contains("not available over a direct connection", run.Result.StdErr + run.Result.StdOut, StringComparison.Ordinal);
        Assert.Empty(handler.Requests);
        Assert.Empty(run.Relay.Invocations);
    }

    [Fact]
    public async Task RemoteHost_AddsSecurityWarningToResults()
    {
        var handler = FakeDirectHandler.Json("""{"data":{"state":"IDLE"}}""");
        var run = await RunAsync(handler, ["downloads", "status", "--json"], profile: new ProfileRecord
        {
            Mode = "direct",
            Host = "192.168.1.20",
            Port = 3128,
            AllowRemote = true,
        });

        Assert.Equal(0, run.Result.ExitCode);
        Assert.Equal("http://192.168.1.20:3128/downloadcontroller/getCurrentState", Assert.Single(handler.Requests).Uri.ToString());
        using var document = CliTestHarness.ParseJson(run.Result.StdOut);
        Assert.Contains("no authentication", document.RootElement.GetProperty("meta").GetProperty("warnings")[0].GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ConnectionTest_ReportsEachProbe()
    {
        var handler = FakeDirectHandler.Json("""{"data":"ok"}""");
        var run = await RunAsync(handler, ["connection", "test", "--json"]);

        Assert.Equal(0, run.Result.ExitCode);
        Assert.Equal(
            ["/device/ping", "/jd/version", "/jd/getCoreRevision", "/downloadcontroller/getCurrentState"],
            handler.Requests.Select(request => request.Uri.AbsolutePath));
        using var document = CliTestHarness.ParseJson(run.Result.StdOut);
        Assert.Equal(4, document.RootElement.GetProperty("data").GetProperty("steps").GetArrayLength());
    }

    [Fact]
    public async Task ConnectionTest_FailureNamesTheFailingStepAndKeepsRecovery()
    {
        var handler = FakeDirectHandler.Throwing(FakeDirectHandler.ConnectionRefused());
        var run = await RunAsync(handler, ["connection", "test", "--json"]);

        Assert.Equal(8, run.Result.ExitCode);
        using var document = CliTestHarness.ParseJson(run.Result.StdOut);
        var error = document.RootElement.GetProperty("error");
        Assert.Contains("failed at 'ping'", error.GetProperty("message").GetString(), StringComparison.Ordinal);
        Assert.Contains("deprecatedapienabled", error.GetProperty("recovery").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task MyJdProfiles_StillUseTheRelayClient()
    {
        var handler = FakeDirectHandler.Json("{}");
        var run = await RunAsync(
            handler,
            ["downloads", "status", "--json"],
            profile: new ProfileRecord { AccountEmail = "a@b.c", DefaultDeviceId = "dev1", DefaultDeviceName = "dev", KnownDevices = [new KnownDeviceRecord { Id = "dev1", Name = "dev" }] },
            relayResponse: new Dictionary<string, object?> { ["state"] = "IDLE" });

        Assert.Equal(0, run.Result.ExitCode);
        Assert.Empty(handler.Requests);
        var invocation = Assert.Single(run.Relay.Invocations);
        Assert.Equal("/downloadcontroller/getCurrentState", invocation.Endpoint);
    }

    private sealed record Run(CliTestHarness.CommandRunResult Result, RecordingRelayClient Relay);

    private static async Task<Run> RunAsync(FakeDirectHandler handler, string[] args, ProfileRecord? profile = null, object? relayResponse = null)
    {
        var root = Path.Combine(Path.GetTempPath(), "jd2-cli-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var env = new FakeCliEnvironment(root);
        var config = new Jd2Config { DefaultProfile = "p" };
        config.Profiles["p"] = profile ?? new ProfileRecord { Mode = "direct", Host = "127.0.0.1", Port = 3128 };
        await CliTestHarness.WriteConfigAsync(new CliPathProvider(env).GetConfigFilePath(), config);

        var relay = new RecordingRelayClient { Response = relayResponse };
        var result = await CliTestHarness.RunAsync(env, args, configureServices: handler.Configure(relay));
        return new Run(result, relay);
    }
}
