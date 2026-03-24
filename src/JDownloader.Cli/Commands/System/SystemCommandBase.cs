using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.System;

public abstract class SystemCommandBase : DeviceApiCommand<DeviceNoArgSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    protected SystemCommandBase(
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

    protected abstract string Operation { get; }
    protected abstract string Endpoint { get; }
    protected virtual string Method => "POST";
    protected virtual bool Destructive => false;
    protected virtual bool ProducesBinary => false;

    protected override async Task<CommandOutput> ExecuteCoreAsync(
        CommandContext context,
        DeviceNoArgSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        var plan = new MyJdRequestPlan(
            Operation,
            Method,
            Endpoint,
            null,
            null,
            Destructive,
            ProducesBinary,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        if (plan.Destructive)
            await _confirmationGuard.AuthorizeAsync(settings, $"'{context.Name}' will execute a destructive remote operation.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(
            result.Data,
            HumanDataRenderer.Render(result.Data),
            result.Warnings);
    }
}
