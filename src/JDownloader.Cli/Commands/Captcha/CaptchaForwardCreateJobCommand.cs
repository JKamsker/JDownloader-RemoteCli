using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Captcha;

public sealed class CaptchaForwardCreateJobSettings : DeviceCommandSettings
{
    [CommandOption("--arg1 <TEXT>")]
    [Description("Provider-specific RecaptchaV2 argument 1.")]
    public string? Arg1 { get; init; }

    [CommandOption("--arg2 <TEXT>")]
    [Description("Provider-specific RecaptchaV2 argument 2.")]
    public string? Arg2 { get; init; }

    [CommandOption("--arg3 <TEXT>")]
    [Description("Provider-specific RecaptchaV2 argument 3.")]
    public string? Arg3 { get; init; }

    [CommandOption("--arg4 <TEXT>")]
    [Description("Provider-specific RecaptchaV2 argument 4.")]
    public string? Arg4 { get; init; }
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
        if (string.IsNullOrWhiteSpace(settings.Arg1)
            || string.IsNullOrWhiteSpace(settings.Arg2)
            || string.IsNullOrWhiteSpace(settings.Arg3)
            || string.IsNullOrWhiteSpace(settings.Arg4))
        {
            throw CliException.Usage("captcha forward create-job requires --arg1 <text> --arg2 <text> --arg3 <text> --arg4 <text>.");
        }

        var plan = new MyJdRequestPlan(
            "captcha.forward.create-job",
            "POST",
            "/captchaforward/createJobRecaptchaV2",
            new Dictionary<string, object?>
            {
                ["arg1"] = settings.Arg1.Trim(),
                ["arg2"] = settings.Arg2.Trim(),
                ["arg3"] = settings.Arg3.Trim(),
                ["arg4"] = settings.Arg4.Trim(),
            },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(
            settings,
            "'captcha forward create-job' will create a provider-specific RecaptchaV2 forward job.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}
