using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Grabber;

public sealed class GrabberJobsListSettings : DeviceCommandSettings
{
    [CommandOption("--job-id <ID>")]
    [Description("Repeatable crawler job id filter.")]
    public long[] JobIds { get; init; } = [];
}

public sealed class GrabberJobsListCommand : DeviceApiCommand<GrabberJobsListSettings>
{
    private readonly IMyJdTransport _transport;

    public GrabberJobsListCommand(
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
        GrabberJobsListSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, object?>
        {
            ["collectorInfo"] = true,
        };
        if (settings.JobIds.Length > 0)
            query["jobIds"] = settings.JobIds;

        var plan = new MyJdRequestPlan(
            "grabber.jobs.list",
            "POST",
            "/linkgrabberv2/queryLinkCrawlerJobs",
            query,
            null,
            false,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}

