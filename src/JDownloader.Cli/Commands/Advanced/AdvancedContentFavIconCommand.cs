using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Advanced;

public sealed class AdvancedContentFavIconSettings : DeviceCommandSettings
{
    [CommandOption("--hoster <NAME>")]
    [Description("Hoster name to fetch the favicon for.")]
    public string? Hoster { get; init; }

    [CommandOption("--output-file <PATH>")]
    [Description("Destination file for the binary response.")]
    public string? OutputFile { get; init; }
}

public sealed class AdvancedContentFavIconCommand : DeviceApiCommand<AdvancedContentFavIconSettings>
{
    private readonly IMyJdTransport _transport;

    public AdvancedContentFavIconCommand(
        IProfileResolver profileResolver,
        IOutputRenderer outputRenderer,
        IDiagnosticLogger diagnosticLogger,
        IMyJdTransport transport)
        : base(profileResolver, outputRenderer, diagnosticLogger)
    {
        _transport = transport;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(
        CommandContext context,
        AdvancedContentFavIconSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Hoster))
            throw CliException.Usage("advanced content favicon requires --hoster <name>.");
        if (string.IsNullOrWhiteSpace(settings.OutputFile))
            throw CliException.Usage("advanced content favicon requires --output-file <path>.");

        var plan = new MyJdRequestPlan(
            "advanced.content.favicon",
            "POST",
            "/contentV2/getFavIcon",
            new Dictionary<string, object?> { ["hostername"] = settings.Hoster.Trim() },
            null,
            false,
            true,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan, settings.OutputFile);

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        var bytes = BinaryData.DecodeBytesOrThrow(result.Data, "advanced content favicon");
        BinaryData.WriteAllBytes(settings.OutputFile, bytes);

        var data = new { outputFile = Path.GetFullPath(settings.OutputFile.Trim()), bytesWritten = bytes.Length };
        return new CommandOutput(
            data,
            [$"Wrote {bytes.Length} bytes to '{data.outputFile}'."],
            result.Warnings);
    }
}
