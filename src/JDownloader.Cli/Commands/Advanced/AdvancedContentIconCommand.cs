using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Advanced;

public sealed class AdvancedContentIconSettings : DeviceCommandSettings
{
    [CommandOption("--key <KEY>")]
    [Description("Icon key to fetch.")]
    public string? Key { get; init; }

    [CommandOption("--size <PX>")]
    [Description("Icon size in pixels.")]
    public int Size { get; init; } = 32;

    [CommandOption("--output-file <PATH>")]
    [Description("Destination file for the binary response.")]
    public string? OutputFile { get; init; }
}

public sealed class AdvancedContentIconCommand : DeviceApiCommand<AdvancedContentIconSettings>
{
    private readonly IMyJdTransport _transport;

    public AdvancedContentIconCommand(
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
        AdvancedContentIconSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Key))
            throw CliException.Usage("advanced content icon requires --key <key>.");
        if (settings.Size <= 0)
            throw CliException.Usage("advanced content icon requires --size <px> to be a positive integer.");
        if (string.IsNullOrWhiteSpace(settings.OutputFile))
            throw CliException.Usage("advanced content icon requires --output-file <path>.");

        var plan = new MyJdRequestPlan(
            "advanced.content.icon",
            "POST",
            "/contentV2/getIcon",
            new Dictionary<string, object?>
            {
                ["key"] = settings.Key.Trim(),
                ["size"] = settings.Size,
            },
            null,
            false,
            true,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan, settings.OutputFile);

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        var bytes = BinaryData.DecodeBytesOrThrow(result.Data, "advanced content icon");
        BinaryData.WriteAllBytes(settings.OutputFile, bytes);

        var data = new { outputFile = Path.GetFullPath(settings.OutputFile.Trim()), bytesWritten = bytes.Length };
        return new CommandOutput(
            data,
            [$"Wrote {bytes.Length} bytes to '{data.outputFile}'."],
            result.Warnings);
    }
}
