using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Settings;

public sealed class SettingsExtensionsInstallSettings : DeviceCommandSettings
{
    [CommandOption("--id <ID>")]
    [Description("Extension id to install.")]
    public string? Id { get; init; }
}

public sealed class SettingsExtensionsInstallCommand : DeviceApiCommand<SettingsExtensionsInstallSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public SettingsExtensionsInstallCommand(
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
        SettingsExtensionsInstallSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Id))
            throw CliException.Usage("settings extensions install requires --id <id>.");

        var id = settings.Id.Trim();
        var plan = new MyJdRequestPlan(
            "settings.extensions.install",
            "POST",
            "/extensions/install",
            new Dictionary<string, object?> { ["id"] = id },
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(settings, $"'settings extensions install' will install extension '{id}'.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }
}

