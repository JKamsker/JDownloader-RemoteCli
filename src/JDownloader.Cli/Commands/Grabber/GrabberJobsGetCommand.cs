using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Grabber;

public sealed class GrabberJobsGetSettings : DeviceCommandSettings
{
    [CommandOption("--job-id <ID>")]
    [Description("Repeatable crawler job id to fetch.")]
    public long[] JobIds { get; init; } = [];
}

public sealed class GrabberJobsGetCommand : DeviceApiCommand<GrabberJobsGetSettings>
{
    private readonly IMyJdTransport _transport;

    public GrabberJobsGetCommand(
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
        GrabberJobsGetSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.JobIds.Length == 0)
            throw CliException.Usage("grabber jobs get requires at least one --job-id <id>.");

        var plan = new MyJdRequestPlan(
            "grabber.jobs.get",
            "POST",
            "/linkgrabberv2/queryLinkCrawlerJobs",
            new Dictionary<string, object?>
            {
                ["collectorInfo"] = true,
                ["jobIds"] = settings.JobIds,
            },
            null,
            false,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(
            result.Data,
            HumanDataRenderer.Render(result.Data),
            result.Warnings);
    }
}

