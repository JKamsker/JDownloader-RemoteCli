using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Advanced;

public sealed class RawRequestSettings : DeviceCommandSettings
{
    [CommandArgument(0, "<ENDPOINT>")]
    [Description("My.JDownloader endpoint path (or full URL). Example: /downloadsV2/queryLinks.")]
    public required string Path { get; init; }

    [CommandOption("--query-json <JSON>")]
    [Description("Raw query JSON or @file.")]
    public string? QueryJson { get; init; }

    [CommandOption("--body-json <JSON>")]
    [Description("Raw body JSON or @file.")]
    public string? BodyJson { get; init; }

    [CommandOption("--output-file <PATH>")]
    [Description("Destination for binary response modes.")]
    public string? OutputFile { get; init; }

    [CommandOption("--destructive")]
    [Description("Mark this call as destructive and require confirmation (unless -y/--yes).")]
    public bool Destructive { get; init; }
}

public sealed class AdvancedRawRequestCommand : DeviceApiCommand<RawRequestSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public AdvancedRawRequestCommand(
        IProfileResolver profileResolver,
        IOutputRenderer outputRenderer,
        IDiagnosticLogger diagnosticLogger,
        IMyJdTransport transport,
        IConfirmationGuard confirmationGuard)
        : base(profileResolver, outputRenderer, diagnosticLogger)
    {
        _transport = transport;
        _confirmationGuard = confirmationGuard;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(CommandContext context, RawRequestSettings settings, ResolvedProfileContext resolved, CancellationToken cancellationToken)
    {
        var producesBinary = !string.IsNullOrWhiteSpace(settings.OutputFile);

        var endpoint = NormalizeEndpoint(settings.Path);
        var plan = new MyJdRequestPlan(
            "advanced.raw.request",
            "POST",
            endpoint,
            JsonInput.ParseOptional(settings.QueryJson),
            JsonInput.ParseOptional(settings.BodyJson),
            Destructive: settings.Destructive,
            ProducesBinary: producesBinary,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        if (plan.Destructive)
        {
            await _confirmationGuard.AuthorizeAsync(
                settings,
                $"'advanced raw request' will execute a destructive call to '{plan.Endpoint}'.");
        }

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);

        if (producesBinary)
        {
            var bytes = BinaryData.DecodeBytesOrThrow(result.Data, "advanced raw request");
            BinaryData.WriteAllBytes(settings.OutputFile!, bytes);

            var data = new { outputFile = Path.GetFullPath(settings.OutputFile!.Trim()), bytesWritten = bytes.Length };
            return new CommandOutput(
                data,
                [
                    $"Path: {plan.Endpoint}",
                    $"Profile: {resolved.ProfileName}",
                    $"Device: {resolved.Device?.DisplayValue ?? "(none)"}",
                    $"Wrote {bytes.Length} bytes to '{data.outputFile}'.",
                ],
                result.Warnings);
        }

        var humanLines = new List<string>
        {
            $"Path: {plan.Endpoint}",
            $"Profile: {resolved.ProfileName}",
            $"Device: {resolved.Device?.DisplayValue ?? "(none)"}",
        };

        var renderedData = HumanDataRenderer.Render(result.Data);
        if (renderedData is { Count: > 0 })
        {
            humanLines.Add(string.Empty);
            humanLines.AddRange(renderedData);
        }

        return new CommandOutput(
            result.Data,
            humanLines,
            result.Warnings);
    }

    private static string NormalizeEndpoint(string rawPath)
    {
        if (string.IsNullOrWhiteSpace(rawPath))
            throw CliException.Usage("advanced raw request requires <PATH>.");

        var trimmed = rawPath.Trim();

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.AbsolutePath))
            trimmed = uri.AbsolutePath;

        var queryIndex = trimmed.IndexOfAny(['?', '#']);
        if (queryIndex >= 0)
            trimmed = trimmed[..queryIndex];

        if (!trimmed.StartsWith('/'))
            trimmed = "/" + trimmed;

        return trimmed;
    }
}
