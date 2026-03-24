using System.ComponentModel;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Advanced;

public sealed class AdvancedIngestCnlSettings : DeviceCommandSettings
{
    [CommandOption("--cnl-json <JSON>")]
    [Description("Full CNL query object JSON or @file override.")]
    public string? CnlJson { get; init; }

    [CommandOption("--urls <TEXT>")]
    [Description("Plain-text URLs for the Click'n'Load payload.")]
    public string? Urls { get; init; }

    [CommandOption("--crypted <TEXT>")]
    [Description("Optional crypted CNL payload.")]
    public string? Crypted { get; init; }

    [CommandOption("--jk <TEXT>")]
    [Description("Optional Click'n'Load jk value.")]
    public string? Jk { get; init; }

    [CommandOption("--key <TEXT>")]
    [Description("Optional Click'n'Load key value.")]
    public string? Key { get; init; }

    [CommandOption("--package-name <NAME>")]
    [Description("Optional package name override.")]
    public string? PackageName { get; init; }

    [CommandOption("--dir <PATH>")]
    [Description("Optional destination directory hint.")]
    public string? Directory { get; init; }

    [CommandOption("--comment <TEXT>")]
    [Description("Optional comment for the ingested package.")]
    public string? Comment { get; init; }

    [CommandOption("--source <TEXT>")]
    [Description("Optional source label sent with the payload.")]
    public string? Source { get; init; }

    [CommandOption("--referrer <URL>")]
    [Description("Optional referrer URL.")]
    public string? Referrer { get; init; }

    [CommandOption("--org-referrer <URL>")]
    [Description("Optional original referrer URL.")]
    public string? OriginalReferrer { get; init; }

    [CommandOption("--org-source <TEXT>")]
    [Description("Optional original source label.")]
    public string? OriginalSource { get; init; }

    [CommandOption("--password <PASSWORD>")]
    [Description("Repeatable extraction password to attach to the CNL payload.")]
    public string[] Passwords { get; init; } = [];

    [CommandOption("--permission")]
    [Description("Set the Click'n'Load permission flag.")]
    public bool Permission { get; init; }
}

public sealed class AdvancedIngestCnlCommand : DeviceApiCommand<AdvancedIngestCnlSettings>
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    public AdvancedIngestCnlCommand(
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
        AdvancedIngestCnlSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        var cnl = BuildCnlPayload(settings, redactPasswords: settings.DryRun);
        var plan = new MyJdRequestPlan(
            "advanced.ingest.cnl",
            "POST",
            "/flash/addcnl",
            cnl,
            null,
            true,
            false,
            resolved.Device?.Id);

        if (settings.DryRun)
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

        await _confirmationGuard.AuthorizeAsync(settings, "'advanced ingest cnl' will add a Click'n'Load payload to Linkgrabber.");

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(result.Data, HumanDataRenderer.Render(result.Data), result.Warnings);
    }

    private static object BuildCnlPayload(AdvancedIngestCnlSettings settings, bool redactPasswords)
    {
        if (!string.IsNullOrWhiteSpace(settings.CnlJson))
        {
            EnsureNoStructuredOverrides(settings);
            var parsed = JsonInput.ParseOptional(settings.CnlJson);
            if (parsed is not Dictionary<string, object?> values)
                throw CliException.Usage("advanced ingest cnl requires --cnl-json to resolve to a JSON object.");

            if (!HasAnyPayload(values))
                throw CliException.Usage("advanced ingest cnl requires a non-empty CNL payload.");

            return redactPasswords ? RedactPasswords(values) : values;
        }

        var cnl = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        AddIfPresent(cnl, "urls", settings.Urls);
        AddIfPresent(cnl, "crypted", settings.Crypted);
        AddIfPresent(cnl, "jk", settings.Jk);
        AddIfPresent(cnl, "key", settings.Key);
        AddIfPresent(cnl, "packageName", settings.PackageName);
        AddIfPresent(cnl, "dir", settings.Directory);
        AddIfPresent(cnl, "comment", settings.Comment);
        AddIfPresent(cnl, "source", settings.Source);
        AddIfPresent(cnl, "referrer", settings.Referrer);
        AddIfPresent(cnl, "orgReferrer", settings.OriginalReferrer);
        AddIfPresent(cnl, "orgSource", settings.OriginalSource);
        if (settings.Passwords.Length > 0)
            cnl["passwords"] = redactPasswords ? settings.Passwords.Select(_ => SecretInput.Redacted).ToArray() : settings.Passwords;
        if (settings.Permission)
            cnl["permission"] = true;

        if (!HasAnyPayload(cnl))
        {
            throw CliException.Usage(
                "advanced ingest cnl requires --cnl-json <json> or at least one CNL field such as --urls <text> or --crypted <text>.");
        }

        return cnl;
    }

    private static void EnsureNoStructuredOverrides(AdvancedIngestCnlSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Urls)
            || !string.IsNullOrWhiteSpace(settings.Crypted)
            || !string.IsNullOrWhiteSpace(settings.Jk)
            || !string.IsNullOrWhiteSpace(settings.Key)
            || !string.IsNullOrWhiteSpace(settings.PackageName)
            || !string.IsNullOrWhiteSpace(settings.Directory)
            || !string.IsNullOrWhiteSpace(settings.Comment)
            || !string.IsNullOrWhiteSpace(settings.Source)
            || !string.IsNullOrWhiteSpace(settings.Referrer)
            || !string.IsNullOrWhiteSpace(settings.OriginalReferrer)
            || !string.IsNullOrWhiteSpace(settings.OriginalSource)
            || settings.Passwords.Length > 0
            || settings.Permission)
        {
            throw CliException.Usage("Do not combine --cnl-json with individual CNL flags.");
        }
    }

    private static bool HasAnyPayload(IReadOnlyDictionary<string, object?> values) => values.Count > 0;

    private static Dictionary<string, object?> RedactPasswords(Dictionary<string, object?> values)
    {
        var result = new Dictionary<string, object?>(values, StringComparer.OrdinalIgnoreCase);
        if (result.TryGetValue("passwords", out var rawPasswords) && rawPasswords is IEnumerable<object?> items)
            result["passwords"] = items.Select(_ => SecretInput.Redacted).ToArray();

        return result;
    }

    private static void AddIfPresent(Dictionary<string, object?> target, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            target[key] = value.Trim();
    }
}
