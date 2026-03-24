using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Captcha;

public sealed class CaptchaSolveSettings : DeviceCommandSettings
{
    [CommandOption("--id <ID>")]
    [Description("Captcha identifier to solve.")]
    public long? Id { get; init; }

    [CommandOption("--result <TEXT>")]
    [Description("Captcha solution/result.")]
    public string? Result { get; init; }

    [CommandOption("--result-format <FORMAT>")]
    [Description("Optional result format.")]
    public string? ResultFormat { get; init; }
}

public sealed class CaptchaSolveCommand : DeviceApiCommand<CaptchaSolveSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public CaptchaSolveCommand(
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
        CaptchaSolveSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.Id is null || string.IsNullOrWhiteSpace(settings.Result))
            throw CliException.Usage("captcha solve requires --id <id> --result <text>.");

        var query = new Dictionary<string, object?>
        {
            ["id"] = settings.Id.Value,
            ["result"] = settings.Result.Trim(),
        };
        if (!string.IsNullOrWhiteSpace(settings.ResultFormat))
            query["resultFormat"] = settings.ResultFormat.Trim();

        var plan = new MyJdRequestPlan(
            "captcha.solve",
            "POST",
            "/captcha/solve",
            query,
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(settings, $"'captcha solve' will submit a solution for captcha {settings.Id.Value}.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}

