using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Captcha;

public sealed class CaptchaGetSettings : DeviceCommandSettings
{
    [CommandOption("--id <ID>")]
    [Description("Captcha identifier to fetch.")]
    public long? Id { get; init; }

    [CommandOption("--format <FORMAT>")]
    [Description("Optional format override.")]
    public string? Format { get; init; }
}

public sealed class CaptchaGetCommand : DeviceApiCommand<CaptchaGetSettings>
{
    private readonly IMyJdTransport _transport;

    public CaptchaGetCommand(
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
        CaptchaGetSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.Id is null)
            throw CliException.Usage("captcha get requires --id <id>.");

        var query = new Dictionary<string, object?> { ["id"] = settings.Id.Value };
        if (!string.IsNullOrWhiteSpace(settings.Format))
            query["format"] = settings.Format.Trim();

        var plan = new MyJdRequestPlan(
            "captcha.get",
            "POST",
            "/captcha/get",
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

