using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;

namespace JDownloader.Cli.Tests;

public sealed class OutputAndPreviewTests
{
    [Fact]
    public void HumanDataRendererFormatsTablesAndByteFields()
    {
        var lines = HumanDataRenderer.Render(
            new[]
            {
                new Dictionary<string, object?>
                {
                    ["name"] = "Package A",
                    ["bytesTotal"] = 2048L,
                    ["speed"] = 1024L,
                    ["status"] = "RUNNING",
                },
            });

        Assert.NotNull(lines);
        var table = lines!;
        Assert.Contains(table, line => line.Contains("bytesTotal", StringComparison.Ordinal));
        Assert.Contains(table, line => line.Contains("2", StringComparison.Ordinal) && line.Contains("KB", StringComparison.Ordinal));
        Assert.Contains(table, line => line.Contains("KB/s", StringComparison.Ordinal));
    }

    [Fact]
    public void PreviewOutputIncludesMetadataAndRenderedSections()
    {
        var output = RequestPlanCommandBase.BuildPreviewOutput(
            CreateResolved(),
            new MyJdRequestPlan(
                "advanced.raw.request",
                "POST",
                "/jd/sum",
                new List<object?> { 1L, 2L },
                new Dictionary<string, object?> { ["note"] = "extra" },
                true,
                true,
                DeviceId: "dev-1",
                PreserveRawParameters: true),
            "artifacts\\result.bin");

        Assert.NotNull(output.HumanLines);
        var lines = output.HumanLines!;
        Assert.Contains("Destructive: yes", lines);
        Assert.Contains("Binary response: yes", lines);
        Assert.Contains(lines, line => line.StartsWith("Output file:", StringComparison.Ordinal));
        Assert.Contains("Query:", lines);
        Assert.Contains("Body:", lines);
    }

    [Fact]
    public void QuietSuppressesWarningsAndVerboseControlsUnexpectedDiagnosticPath()
    {
        var renderer = new OutputRenderer();
        var quietResult = CaptureConsole(() => renderer.WriteAnonymousSuccess(
            OutputMode.Human,
            new CommandOutput(new { ok = true }, ["done"], ["careful"]),
            quiet: true));

        Assert.Equal("done" + Environment.NewLine, quietResult.StdOut);
        Assert.True(string.IsNullOrWhiteSpace(quietResult.StdErr));

        var noisyResult = CaptureConsole(() => renderer.WriteUnexpectedFailure(
            OutputMode.Human,
            new InvalidOperationException("boom"),
            "logs\\diag.log",
            verbose: false,
            quiet: false));
        Assert.DoesNotContain("diag.log", noisyResult.StdErr, StringComparison.Ordinal);

        var verboseResult = CaptureConsole(() => renderer.WriteUnexpectedFailure(
            OutputMode.Human,
            new InvalidOperationException("boom"),
            "logs\\diag.log",
            verbose: true,
            quiet: false));
        Assert.Contains("diag.log", verboseResult.StdErr, StringComparison.Ordinal);
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

    private static (string StdOut, string StdErr) CaptureConsole(Action action)
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        var originalOut = Console.Out;
        var originalErr = Console.Error;

        try
        {
            Console.SetOut(stdout);
            Console.SetError(stderr);
            action();
            return (stdout.ToString(), stderr.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
    }
}
