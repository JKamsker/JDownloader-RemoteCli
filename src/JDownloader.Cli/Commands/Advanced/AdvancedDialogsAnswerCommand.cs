using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Advanced;

public sealed class AdvancedDialogsAnswerSettings : DeviceCommandSettings
{
    [CommandOption("--id <ID>")]
    [Description("Dialog id to answer.")]
    public long? Id { get; init; }

    [CommandOption("--data-json <JSON>")]
    [Description("Answer payload as JSON object or @file.")]
    public string? DataJson { get; init; }
}

public sealed class AdvancedDialogsAnswerCommand : DeviceApiCommand<AdvancedDialogsAnswerSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public AdvancedDialogsAnswerCommand(
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
        AdvancedDialogsAnswerSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (settings.Id is null)
            throw CliException.Usage("advanced dialogs answer requires --id <id>.");

        var data = JsonInput.ParseOptional(settings.DataJson);
        if (data is not Dictionary<string, object?>)
            throw CliException.Usage("advanced dialogs answer requires --data-json <json-object-or-@file>.");

        var plan = new MyJdRequestPlan(
            "advanced.dialogs.answer",
            "POST",
            "/dialogs/answer",
            new Dictionary<string, object?>
            {
                ["id"] = settings.Id.Value,
                ["data"] = data,
            },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(
            settings,
            $"'advanced dialogs answer' will answer dialog {settings.Id.Value}.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(
            result.Data,
            HumanDataRenderer.Render(result.Data),
            result.Warnings);
    }
}
