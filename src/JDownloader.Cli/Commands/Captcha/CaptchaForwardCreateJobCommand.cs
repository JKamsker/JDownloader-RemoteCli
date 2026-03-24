using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Captcha;

public sealed class CaptchaForwardCreateJobSettings : DeviceCommandSettings
{
    [CommandArgument(0, "<ARG1>")]
    public required string Arg1 { get; init; }

    [CommandArgument(1, "<ARG2>")]
    public required string Arg2 { get; init; }

    [CommandArgument(2, "<ARG3>")]
    public required string Arg3 { get; init; }

    [CommandArgument(3, "<ARG4>")]
    public required string Arg4 { get; init; }
}

public sealed class CaptchaForwardCreateJobCommand : DeviceApiCommand<CaptchaForwardCreateJobSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public CaptchaForwardCreateJobCommand(
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
        CaptchaForwardCreateJobSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        var plan = new MyJdRequestPlan(
            "captcha.forward.create-job",
            "POST",
            "/captchaforward/createJobRecaptchaV2",
            new Dictionary<string, object?>
            {
                ["arg1"] = settings.Arg1,
                ["arg2"] = settings.Arg2,
                ["arg3"] = settings.Arg3,
                ["arg4"] = settings.Arg4,
            },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(settings, "'captcha forward create-job' will create a captcha forward job.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}

