using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Grabber;

public sealed class GrabberAddSettings : DeviceCommandSettings
{
    [CommandOption("--url <URL>")]
    [Description("Repeatable URL to add to the linkgrabber.")]
    public string[] Urls { get; init; } = [];

    [CommandOption("--links <TEXT>")]
    [Description("Raw newline-separated link text to add.")]
    public string? Links { get; init; }

    [CommandOption("--package-name <NAME>")]
    [Description("Optional package name override.")]
    public string? PackageName { get; init; }

    [CommandOption("--destination-folder <PATH>")]
    [Description("Optional destination folder override.")]
    public string? DestinationFolder { get; init; }

    [CommandOption("--source-url <URL>")]
    [Description("Optional source URL for provenance.")]
    public string? SourceUrl { get; init; }

    [CommandOption("--autostart")]
    [Description("Set AddLinksQuery.autostart=true.")]
    public bool Autostart { get; init; }

    [CommandOption("--deep-decrypt")]
    [Description("Set AddLinksQuery.deepDecrypt=true.")]
    public bool DeepDecrypt { get; init; }

    [CommandOption("--auto-extract")]
    [Description("Set AddLinksQuery.autoExtract=true.")]
    public bool AutoExtract { get; init; }

    [CommandOption("--overwrite-packagizer-rules")]
    [Description("Set AddLinksQuery.overwritePackagizerRules=true.")]
    public bool OverwritePackagizerRules { get; init; }

    [CommandOption("--assign-job-id")]
    [Description("Set AddLinksQuery.assignJobID=true.")]
    public bool AssignJobId { get; init; }

    [CommandOption("--query-json <JSON>")]
    [Description("Raw AddLinksQuery JSON object or @file override. Do not combine with other flags.")]
    public string? QueryJson { get; init; }
}

public sealed class GrabberAddCommand : DeviceApiCommand<GrabberAddSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public GrabberAddCommand(
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
        GrabberAddSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        var query = BuildQuery(settings);
        var plan = new MyJdRequestPlan(
            "grabber.add",
            "POST",
            "/linkgrabberv2/addLinks",
            query,
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(settings, "'grabber add' will add links to the linkgrabber.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }

    private static Dictionary<string, object?> BuildQuery(GrabberAddSettings settings)
    {
        var overrideQuery = JsonInput.ParseOptional(settings.QueryJson);
        if (overrideQuery is not null)
        {
            if (settings.Urls.Length > 0
                || !string.IsNullOrWhiteSpace(settings.Links)
                || !string.IsNullOrWhiteSpace(settings.PackageName)
                || !string.IsNullOrWhiteSpace(settings.DestinationFolder)
                || !string.IsNullOrWhiteSpace(settings.SourceUrl)
                || settings.Autostart
                || settings.DeepDecrypt
                || settings.AutoExtract
                || settings.OverwritePackagizerRules
                || settings.AssignJobId)
            {
                throw CliException.Usage("Do not combine --query-json with other 'grabber add' flags. Put the full AddLinksQuery object in --query-json or omit it.");
            }

            if (overrideQuery is not Dictionary<string, object?> obj)
                throw CliException.Usage("--query-json must resolve to a JSON object for grabber add.");

            return obj;
        }

        var parts = new List<string>();
        foreach (var url in settings.Urls)
        {
            if (!string.IsNullOrWhiteSpace(url))
                parts.Add(url.Trim());
        }
        if (!string.IsNullOrWhiteSpace(settings.Links))
            parts.Add(settings.Links.Trim());

        if (parts.Count == 0)
            throw CliException.Usage("grabber add requires at least one --url <url> or --links <text>.");

        var query = new Dictionary<string, object?>
        {
            ["links"] = string.Join(Environment.NewLine, parts),
        };

        if (!string.IsNullOrWhiteSpace(settings.PackageName))
            query["packageName"] = settings.PackageName.Trim();
        if (!string.IsNullOrWhiteSpace(settings.DestinationFolder))
            query["destinationFolder"] = settings.DestinationFolder.Trim();
        if (!string.IsNullOrWhiteSpace(settings.SourceUrl))
            query["sourceUrl"] = settings.SourceUrl.Trim();

        if (settings.Autostart)
            query["autostart"] = true;
        if (settings.DeepDecrypt)
            query["deepDecrypt"] = true;
        if (settings.AutoExtract)
            query["autoExtract"] = true;
        if (settings.OverwritePackagizerRules)
            query["overwritePackagizerRules"] = true;
        if (settings.AssignJobId)
            query["assignJobID"] = true;

        return query;
    }
}
