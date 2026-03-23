using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Advanced;

public sealed class AdvancedContentDescribeIconSettings : DeviceCommandSettings
{
    [CommandOption("--key <KEY>")]
    [Description("Icon key to describe.")]
    public string? Key { get; init; }
}

public sealed class AdvancedContentDescribeIconCommand : DeviceApiCommand<AdvancedContentDescribeIconSettings>
{
    private readonly IMyJdTransport _transport;

    public AdvancedContentDescribeIconCommand(
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
        AdvancedContentDescribeIconSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Key))
            throw CliException.Usage("advanced content describe requires --key <key>.");

        var plan = new MyJdRequestPlan(
            "advanced.content.describe",
            "POST",
            "/contentV2/getIconDescription",
            new Dictionary<string, object?> { ["key"] = settings.Key.Trim() },
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
