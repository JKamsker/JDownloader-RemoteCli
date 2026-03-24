using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Captcha;

public sealed class CaptchaForwardGetResultSettings : DeviceCommandSettings
{
    [CommandOption("--job-id <ID>")]
    [Description("Captcha forward job id to retrieve the result for.")]
    public long? JobId { get; init; }
}

public sealed class CaptchaForwardGetResultCommand : DeviceApiCommand<CaptchaForwardGetResultSettings>
{
    private readonly IMyJdTransport _transport;

    public CaptchaForwardGetResultCommand(
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
        CaptchaForwardGetResultSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.JobId is null)
            throw CliException.Usage("captcha forward get-result requires --job-id <id>.");

        var plan = new MyJdRequestPlan(
            "captcha.forward.get-result",
            "POST",
            "/captchaforward/getResult",
            new Dictionary<string, object?> { ["id"] = settings.JobId.Value },
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

