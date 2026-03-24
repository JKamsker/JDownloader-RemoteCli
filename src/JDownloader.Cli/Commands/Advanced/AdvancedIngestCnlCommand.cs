using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Advanced;

public sealed class AdvancedIngestCnlSettings : DeviceCommandSettings
{
    [CommandOption("--url <URL>")]
    [Description("URL to add to Linkgrabber via the Flash/Toolbar ingest endpoint.")]
    public string? Url { get; init; }

    [CommandOption("--source <NAME>")]
    [Description("Source label sent to the ingest endpoint.")]
    public string Source { get; init; } = "jd2-cli";

    [CommandOption("--password <PASSWORD>")]
    [Description("Optional password passed to the ingest endpoint.")]
    public string? Password { get; init; }
}

public sealed class AdvancedIngestCnlCommand : DeviceApiCommand<AdvancedIngestCnlSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public AdvancedIngestCnlCommand(
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

    protected override async Task<CommandOutput> ExecuteCoreAsync(
        CommandContext context,
        AdvancedIngestCnlSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Url))
            throw CliException.Usage("advanced ingest cnl requires --url <url>.");

        var password = settings.Password?.Trim() ?? string.Empty;
        if (settings.DryRun && !string.IsNullOrWhiteSpace(password))
            password = SecretInput.Redacted;

        var plan = new MyJdRequestPlan(
            "advanced.ingest.cnl",
            "POST",
            "/flash/add",
            new Dictionary<string, object?>
            {
                ["password"] = password,
                ["source"] = string.IsNullOrWhiteSpace(settings.Source) ? "jd2-cli" : settings.Source.Trim(),
                ["url"] = settings.Url.Trim(),
            },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(settings, "'advanced ingest cnl' will add links to Linkgrabber.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(
            result.Data,
            HumanDataRenderer.Render(result.Data),
            result.Warnings);
    }
}
