using System.Text.Json;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Tests.Support;
using JDownloader.Cli.Transport;

namespace JDownloader.Cli.Tests;

public sealed class TransportRequestTests
{
    [Fact]
    public async Task QueryEndpointsApplyFieldAliasesAndReturnWarnings()
    {
        var relayClient = new RecordingRelayClient { Response = new { ok = true } };
        var transport = new LiveMyJdTransport(relayClient);

        var result = await transport.ExecuteAsync(
            CreateResolved(),
            new MyJdRequestPlan(
                "downloads.links.list",
                "POST",
                "/linkgrabberv2/queryLinks",
                new Dictionary<string, object?>
                {
                    ["fields"] = new[] { "variantId", "bogus" },
                    ["packageIds"] = new[] { 1L, 2L },
                },
                null,
                false,
                false),
            CancellationToken.None);

        Assert.Equal("Unknown projection field 'bogus' was ignored.", Assert.Single(result.Warnings!));
        var invocation = Assert.Single(relayClient.Invocations);
        var parameters = Assert.IsType<object?[]>(invocation.Parameters);
        using var document = JsonDocument.Parse(Assert.IsType<string>(parameters[0]));
        Assert.True(document.RootElement.GetProperty("variantID").GetBoolean());
        Assert.Equal(2, document.RootElement.GetProperty("packageUUIDs").GetArrayLength());
    }

    [Fact]
    public async Task BooleanEndpointsCoerceStringFlagsToBooleans()
    {
        var relayClient = new RecordingRelayClient();
        var transport = new LiveMyJdTransport(relayClient);

        await transport.ExecuteAsync(
            CreateResolved(),
            new MyJdRequestPlan(
                "downloads.pause",
                "POST",
                "/downloadcontroller/pause",
                new Dictionary<string, object?> { ["value"] = "false" },
                null,
                false,
                false),
            CancellationToken.None);

        var invocation = Assert.Single(relayClient.Invocations);
        var parameters = Assert.IsType<object?[]>(invocation.Parameters);
        Assert.False(Assert.IsType<bool>(parameters[0]));
    }

    [Fact]
    public async Task PositionalEndpointsPackLinkAndPackageIds()
    {
        var relayClient = new RecordingRelayClient();
        var transport = new LiveMyJdTransport(relayClient);

        await transport.ExecuteAsync(
            CreateResolved(),
            new MyJdRequestPlan(
                "downloads.links.remove",
                "POST",
                "/downloadsV2/removeLinks",
                new Dictionary<string, object?>
                {
                    ["linkIds"] = new[] { "1", "2" },
                    ["packageIds"] = new[] { "3" },
                },
                null,
                true,
                false),
            CancellationToken.None);

        var parameters = Assert.IsType<object?[]>(Assert.Single(relayClient.Invocations).Parameters);
        Assert.Equal([1L, 2L], Assert.IsType<long[]>(parameters[0]));
        Assert.Equal([3L], Assert.IsType<long[]>(parameters[1]));
    }

    [Fact]
    public async Task RawRequestsBypassEndpointMappersAndKeepTopLevelParamArrays()
    {
        var relayClient = new RecordingRelayClient { Response = 42L };
        var transport = new LiveMyJdTransport(relayClient);

        var result = await transport.ExecuteAsync(
            CreateResolved(),
            new MyJdRequestPlan(
                "advanced.raw.request",
                "POST",
                "/flash/add",
                new List<object?> { "password", "source", "https://example.invalid" },
                null,
                false,
                false,
                PreserveRawParameters: true),
            CancellationToken.None);

        Assert.Null(result.Warnings);
        var parameters = Assert.IsAssignableFrom<IEnumerable<object?>>(Assert.Single(relayClient.Invocations).Parameters);
        Assert.Equal(["password", "source", "https://example.invalid"], parameters.Cast<string?>());
    }

    private static ResolvedProfileContext CreateResolved()
    {
        return new(
            "main",
            "flag",
            "user@example.com",
            OutputMode.Human,
            "default",
            30,
            "default",
            new ResolvedDevice("dev-1", "Box"),
            "flag");
    }
}
