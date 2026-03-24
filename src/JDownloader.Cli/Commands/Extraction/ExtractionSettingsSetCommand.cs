using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Extraction;

public sealed class ExtractionSettingsSetSettings : DeviceCommandSettings
{
    [CommandOption("--archive-id <ID>")]
    [Description("Archive identifier to update.")]
    public string? ArchiveId { get; init; }

    [CommandOption("--settings-json <JSON>")]
    [Description("Archive settings JSON object or @file.")]
    public string? SettingsJson { get; init; }
}

public sealed class ExtractionSettingsSetCommand : DeviceApiCommand<ExtractionSettingsSetSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public ExtractionSettingsSetCommand(
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
        ExtractionSettingsSetSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.ArchiveId) || string.IsNullOrWhiteSpace(settings.SettingsJson))
        {
            throw CliException.Usage("extraction settings set requires --archive-id <id> --settings-json <json-or-@file>.");
        }

        var archiveSettings = JsonInput.ParseOptional(settings.SettingsJson);
        if (archiveSettings is not Dictionary<string, object?> settingsObject)
        {
            throw CliException.Usage("--settings-json must resolve to a JSON object.");
        }

        var plan = new MyJdRequestPlan(
            "extraction.settings.set",
            "POST",
            "/extraction/setArchiveSettings",
            new Dictionary<string, object?>
            {
                ["archiveId"] = settings.ArchiveId.Trim(),
                ["archiveSettings"] = settingsObject,
            },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(settings, $"'extraction settings set' will update settings for archive '{settings.ArchiveId.Trim()}'.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}

