using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Captcha;

public sealed class CaptchaForwardCreateJobSettings : DeviceCommandSettings
{
    [CommandArgument(0, "<PARAM1>")]
    [Description("First (unnamed) string parameter for /captchaforward/createJobRecaptchaV2.")]
    public required string Param1 { get; init; }

    [CommandArgument(1, "<PARAM2>")]
    [Description("Second (unnamed) string parameter for /captchaforward/createJobRecaptchaV2.")]
    public required string Param2 { get; init; }

    [CommandArgument(2, "<PARAM3>")]
    [Description("Third (unnamed) string parameter for /captchaforward/createJobRecaptchaV2.")]
    public required string Param3 { get; init; }

    [CommandArgument(3, "<PARAM4>")]
    [Description("Fourth (unnamed) string parameter for /captchaforward/createJobRecaptchaV2.")]
    public required string Param4 { get; init; }
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
                ["arg1"] = settings.Param1,
                ["arg2"] = settings.Param2,
                ["arg3"] = settings.Param3,
                ["arg4"] = settings.Param4,
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
