using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Advanced;

public sealed class AdvancedContentFileIconSettings : DeviceCommandSettings
{
    [CommandOption("--filename <NAME>")]
    [Description("Filename to fetch an icon for.")]
    public string? Filename { get; init; }

    [CommandOption("--output-file <PATH>")]
    [Description("Destination file for the binary response.")]
    public string? OutputFile { get; init; }
}

public sealed class AdvancedContentFileIconCommand : DeviceApiCommand<AdvancedContentFileIconSettings>
{
    private readonly IMyJdTransport _transport;

    public AdvancedContentFileIconCommand(
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
        AdvancedContentFileIconSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Filename))
            throw CliException.Usage("advanced content file-icon requires --filename <name>.");
        if (string.IsNullOrWhiteSpace(settings.OutputFile))
            throw CliException.Usage("advanced content file-icon requires --output-file <path>.");

        var plan = new MyJdRequestPlan(
            "advanced.content.file-icon",
            "POST",
            "/contentV2/getFileIcon",
            new Dictionary<string, object?> { ["filename"] = settings.Filename.Trim() },
            null,
            false,
            true,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        var bytes = BinaryData.DecodeBytesOrThrow(result.Data, "advanced content file-icon");
        BinaryData.WriteAllBytes(settings.OutputFile, bytes);

        var data = new { outputFile = Path.GetFullPath(settings.OutputFile.Trim()), bytesWritten = bytes.Length };
        return new CommandOutput(
            data,
            [$"Wrote {bytes.Length} bytes to '{data.outputFile}'."],
            result.Warnings);
    }
}
