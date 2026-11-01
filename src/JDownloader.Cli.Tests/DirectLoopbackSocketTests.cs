using System.Net;
using System.Net.Sockets;
using System.Text;
using JDownloader.Cli.Config;
using JDownloader.Cli.Tests.Support;

namespace JDownloader.Cli.Tests;

/// <summary>Exercises the real HttpClient stack (no mocked handler) against loopback sockets.</summary>
public sealed class DirectLoopbackSocketTests
{
    [Fact]
    public async Task RealHttpStack_TalksToLocalListener()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var server = ServeOnceAsync(listener, """{"data":"2.0","rid":-1}""");

        var result = await RunAsync(port, ["system", "jd", "version", "--json"]);

        var requestText = await server;
        Assert.Equal(0, result.ExitCode);
        Assert.StartsWith("POST /jd/version HTTP/1.1", requestText, StringComparison.Ordinal);
        Assert.DoesNotContain("Origin:", requestText, StringComparison.OrdinalIgnoreCase);
        using var document = CliTestHarness.ParseJson(result.StdOut);
        Assert.Equal("2.0", document.RootElement.GetProperty("data").GetString());
    }

    [Fact]
    public async Task RealHttpStack_MapsClosedPortToConnectionRefused()
    {
        var closedPort = GetFreePort();

        var result = await RunAsync(closedPort, ["downloads", "status", "--json"]);

        Assert.Equal(8, result.ExitCode);
        Assert.Contains("Connection refused", result.StdOut + result.StdErr, StringComparison.Ordinal);
    }

    private static async Task<CliTestHarness.CommandRunResult> RunAsync(int port, string[] args)
    {
        var root = Path.Combine(Path.GetTempPath(), "jd2-cli-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var env = new FakeCliEnvironment(root);
        var config = new Jd2Config { DefaultProfile = "p" };
        config.Profiles["p"] = new ProfileRecord { Mode = "direct", Host = "127.0.0.1", Port = port, TimeoutSeconds = 10 };
        await CliTestHarness.WriteConfigAsync(new CliPathProvider(env).GetConfigFilePath(), config);
        return await CliTestHarness.RunAsync(env, args);
    }

    private static int GetFreePort()
    {
        using var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        return ((IPEndPoint)probe.LocalEndpoint).Port;
    }

    private static async Task<string> ServeOnceAsync(TcpListener listener, string jsonBody)
    {
        using var client = await listener.AcceptTcpClientAsync();
        await using var stream = client.GetStream();
        var buffer = new byte[8192];
        var received = new StringBuilder();
        while (!received.ToString().Contains("\r\n\r\n", StringComparison.Ordinal))
        {
            var read = await stream.ReadAsync(buffer);
            if (read == 0)
                break;
            received.Append(Encoding.UTF8.GetString(buffer, 0, read));
        }

        var payload = Encoding.UTF8.GetBytes(jsonBody);
        var header = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: {payload.Length}\r\nConnection: close\r\n\r\n";
        await stream.WriteAsync(Encoding.ASCII.GetBytes(header));
        await stream.WriteAsync(payload);
        await stream.FlushAsync();
        return received.ToString();
    }
}
