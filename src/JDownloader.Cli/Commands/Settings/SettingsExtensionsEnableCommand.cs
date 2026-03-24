using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Settings;

public sealed class SettingsExtensionsEnableSettings : DeviceCommandSettings
{
    [CommandOption("--id <ID>")]
    [Description("Extension id to enable.")]
    public string? Id { get; init; }

    [CommandOption("--classname <NAME>")]
    [Description("Extension classname/config interface to enable (alternative to --id).")]
    public string? Classname { get; init; }
}

public sealed class SettingsExtensionsEnableCommand : DeviceApiCommand<SettingsExtensionsEnableSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public SettingsExtensionsEnableCommand(
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
        SettingsExtensionsEnableSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Id) == string.IsNullOrWhiteSpace(settings.Classname))
            throw CliException.Usage("settings extensions enable requires exactly one of --id <id> or --classname <name>.");

        var classname = string.IsNullOrWhiteSpace(settings.Classname)
            ? await ResolveClassnameAsync(settings.Id!.Trim(), resolved, cancellationToken)
            : settings.Classname!.Trim();

        var plan = new MyJdRequestPlan(
            "settings.extensions.enable",
            "POST",
            "/extensions/setEnabled",
            new Dictionary<string, object?> { ["classname"] = classname, ["b"] = true },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(
            settings,
            $"'settings extensions enable' will enable extension '{classname}'.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(
            result.Data,
            HumanDataRenderer.Render(result.Data),
            result.Warnings);
    }

    private async Task<string> ResolveClassnameAsync(string id, ResolvedProfileContext resolved, CancellationToken cancellationToken)
    {
        var result = await _transport.ExecuteAsync(
            resolved,
            new MyJdRequestPlan(
                "settings.extensions.list",
                "POST",
                "/extensions/list",
                new Dictionary<string, object?>(),
                null,
                false,
                false,
                resolved.Device?.Id),
            cancellationToken);

        if (result.Data is not IEnumerable<object?> sequence)
            throw CliException.NotFound("Extension list response was not a sequence.");

        var matches = sequence
            .OfType<Dictionary<string, object?>>()
            .Where(item => item.TryGetValue("id", out var value)
                && value is not null
                && string.Equals(value.ToString(), id, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0)
            throw CliException.NotFound($"Extension id '{id}' was not found.");
        if (matches.Count > 1)
            throw CliException.Conflict($"Extension id '{id}' matched multiple entries.");

        var item = matches[0];
        if (item.TryGetValue("configInterface", out var rawClassname)
            && rawClassname is not null
            && !string.IsNullOrWhiteSpace(rawClassname.ToString()))
        {
            return rawClassname.ToString()!.Trim();
        }

        throw CliException.NotFound($"Extension id '{id}' did not include a config interface/classname.");
    }
}
