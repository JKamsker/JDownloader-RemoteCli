using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Advanced;

public sealed class RawRequestSettings : DeviceCommandSettings
{
    [CommandArgument(0, "<PATH>")]
    public required string Path { get; init; }

    [CommandOption("--method <METHOD>")]
    [Description("HTTP method to plan. Defaults to POST.")]
    public string? Method { get; init; }

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

        if (!string.IsNullOrWhiteSpace(settings.Method)
            && !string.Equals(settings.Method.Trim(), "POST", StringComparison.OrdinalIgnoreCase))
        {
            throw CliException.Usage("My.JDownloader relay device calls are always POST; '--method' only supports POST.");
        }

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
            var proceed = await _confirmationGuard.AuthorizeAsync(
                settings,
                $"'advanced raw request' will execute a destructive call to '{plan.Endpoint}'.");
            if (!proceed)
                return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);
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

        return new CommandOutput(
            result.Data,
            [
                $"Path: {plan.Endpoint}",
                $"Method: {plan.Method}",
                $"Profile: {resolved.ProfileName}",
                $"Device: {resolved.Device?.DisplayValue ?? "(none)"}",
            ],
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
