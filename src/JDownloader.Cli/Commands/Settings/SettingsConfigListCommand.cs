using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Settings;

public sealed class SettingsConfigListSettings : DeviceCommandSettings
{
    [CommandOption("--pattern <TEXT>")]
    [Description("Optional pattern filter.")]
    public string? Pattern { get; init; }

    [CommandOption("--return-description")]
    [Description("Include docs/description fields.")]
    public bool ReturnDescription { get; init; }

    [CommandOption("--return-values")]
    [Description("Include current values.")]
    public bool ReturnValues { get; init; }

    [CommandOption("--return-default-values")]
    [Description("Include default values.")]
    public bool ReturnDefaultValues { get; init; }

    [CommandOption("--return-enum-info")]
    [Description("Include enum metadata.")]
    public bool ReturnEnumInfo { get; init; }
}

public sealed class SettingsConfigListCommand : DeviceApiCommand<SettingsConfigListSettings>
{
    private readonly IMyJdTransport _transport;

    public SettingsConfigListCommand(
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
        SettingsConfigListSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        var hasArgs = !string.IsNullOrWhiteSpace(settings.Pattern)
            || settings.ReturnDescription
            || settings.ReturnValues
            || settings.ReturnDefaultValues
            || settings.ReturnEnumInfo;

        Dictionary<string, object?>? query = null;
        if (hasArgs)
        {
            query = new Dictionary<string, object?>
            {
                ["pattern"] = settings.Pattern?.Trim() ?? string.Empty,
                ["returnDescription"] = settings.ReturnDescription,
                ["returnValues"] = settings.ReturnValues,
                ["returnDefaultValues"] = settings.ReturnDefaultValues,
                ["returnEnumInfo"] = settings.ReturnEnumInfo,
            };
        }

        var plan = new MyJdRequestPlan(
            "settings.config.list",
            "POST",
            "/config/list",
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

