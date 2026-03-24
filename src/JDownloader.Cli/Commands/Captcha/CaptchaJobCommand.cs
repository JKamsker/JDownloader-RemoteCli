using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Captcha;

public sealed class CaptchaJobSettings : DeviceCommandSettings
{
    [CommandOption("--id <ID>")]
    [Description("Captcha identifier to fetch as a job object.")]
    public long? Id { get; init; }
}

public sealed class CaptchaJobCommand : DeviceApiCommand<CaptchaJobSettings>
{
    private readonly IMyJdTransport _transport;

    public CaptchaJobCommand(
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
        CaptchaJobSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.Id is null)
            throw CliException.Usage("captcha job requires --id <id>.");

        var plan = new MyJdRequestPlan(
            "captcha.job",
            "POST",
            "/captcha/getCaptchaJob",
            new Dictionary<string, object?> { ["id"] = settings.Id.Value },
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

