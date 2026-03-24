using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Captcha;

public sealed class CaptchaSkipSettings : DeviceCommandSettings
{
    [CommandOption("--id <ID>")]
    [Description("Captcha identifier to skip.")]
    public long? Id { get; init; }

    [CommandOption("--type <TYPE>")]
    [Description("Optional skip request type (deprecated API may ignore this).")]
    public string? Type { get; init; }
}

public sealed class CaptchaSkipCommand : DeviceApiCommand<CaptchaSkipSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public CaptchaSkipCommand(
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
        CaptchaSkipSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.Id is null)
            throw CliException.Usage("captcha skip requires --id <id>.");

        var query = new Dictionary<string, object?> { ["id"] = settings.Id.Value };
        if (!string.IsNullOrWhiteSpace(settings.Type))
            query["type"] = settings.Type.Trim();

        var plan = new MyJdRequestPlan(
            "captcha.skip",
            "POST",
            "/captcha/skip",
            query,
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(settings, $"'captcha skip' will skip captcha {settings.Id.Value}.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}

