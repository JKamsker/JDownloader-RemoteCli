using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Grabber;

public sealed class GrabberPackagesRemoveSettings : DeviceCommandSettings
{
    [CommandOption("--package-id <ID>")]
    [Description("Repeatable linkgrabber package identifier to remove.")]
    public long[] PackageIds { get; init; } = [];
}

public sealed class GrabberPackagesRemoveCommand : DeviceApiCommand<GrabberPackagesRemoveSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public GrabberPackagesRemoveCommand(
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
        GrabberPackagesRemoveSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.PackageIds.Length == 0)
            throw CliException.Usage("grabber packages remove requires at least one --package-id <id>.");

        var plan = new MyJdRequestPlan(
            "grabber.packages.remove",
            "POST",
            "/linkgrabberv2/removeLinks",
            new Dictionary<string, object?> { ["linkIds"] = Array.Empty<long>(), ["packageIds"] = settings.PackageIds },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(settings, "'grabber packages remove' will remove selected linkgrabber packages.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}
