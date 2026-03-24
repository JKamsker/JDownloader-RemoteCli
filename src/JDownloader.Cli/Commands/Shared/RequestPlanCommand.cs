using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Shared;

public static class RequestPlanCommandBase
{
    public static CommandOutput BuildPreviewOutput(ResolvedProfileContext resolved, MyJdRequestPlan plan)
    {
        var data = new
        {
            action = "dry-run",
            profile = resolved.ProfileName,
            device = resolved.Device is null ? null : new { id = resolved.Device.Id, name = resolved.Device.Name },
            plan.Operation,
            plan.Method,
            plan.Endpoint,
            plan.Query,
            plan.Body,
            plan.Destructive,
            plan.ProducesBinary,
        };

        return new CommandOutput(
            data,
            BuildHumanLines(resolved, plan));
    }

    private static IReadOnlyList<string> BuildHumanLines(ResolvedProfileContext resolved, MyJdRequestPlan plan)
    {
        var lines = new List<string>
        {
                "Dry-run only. No changes were applied.",
                $"Profile: {resolved.ProfileName}",
                $"Device: {resolved.Device?.DisplayValue ?? "(none)"}",
                $"Method: {plan.Method}",
                $"Endpoint: {plan.Endpoint}",
            };

        AppendSection(lines, "Query", plan.Query);
        AppendSection(lines, "Body", plan.Body);
        return lines;
    }

    private static void AppendSection(List<string> lines, string title, object? value)
    {
        if (value is null)
            return;

        var rendered = HumanDataRenderer.Render(value);
        if (rendered is not { Count: > 0 })
            return;

        lines.Add($"{title}:");
        foreach (var line in rendered)
            lines.Add($"  {line}");
    }
}

public abstract class RequestPlanCommandBase<TSettings> : DeviceApiCommand<TSettings>
    where TSettings : DeviceCommandSettings, IRequestPlanSelectorSettings
{
    private readonly IMyJdTransport _transport;
    private readonly IConfirmationGuard _confirmationGuard;

    protected RequestPlanCommandBase(
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

    protected abstract MyJdRequestPlan CreatePlan(CommandContext context, TSettings settings, ResolvedProfileContext resolved);
    protected virtual IReadOnlyList<string>? RenderHumanData(object? data) => HumanDataRenderer.Render(data);

    protected override async Task<CommandOutput> ExecuteCoreAsync(CommandContext context, TSettings settings, ResolvedProfileContext resolved, CancellationToken cancellationToken)
    {
        var plan = CreatePlan(context, settings, resolved);
        if (plan.Destructive)
        {
            if (settings.DryRun)
                return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);

            await _confirmationGuard.AuthorizeAsync(settings, $"'{context.Name}' is destructive.");
        }
        else if (settings.DryRun)
        {
            return RequestPlanCommandBase.BuildPreviewOutput(resolved, plan);
        }

        var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
        return new CommandOutput(
            result.Data,
            RenderHumanData(result.Data),
            result.Warnings);
    }

    protected static Dictionary<string, object?> BuildSelectorQuery(TSettings settings)
    {
        var queryOverride = JsonInput.ParseOptional(settings.QueryJson);
        if (queryOverride is not null)
        {
            if (!string.IsNullOrWhiteSpace(settings.Fields)
                || settings.Limit is not null
                || settings.Offset is not null
                || (settings is RequestCommandSettings withPackages && withPackages.PackageIds.Length > 0))
            {
                throw CliException.Usage("Do not combine selector flags with --query-json. Put the full query object in --query-json or omit it.");
            }

            if (queryOverride is not Dictionary<string, object?> overrideObject)
                throw CliException.Usage("--query-json must resolve to a JSON object for query-style endpoints.");

            return overrideObject;
        }

        var query = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(settings.Fields))
            query["fields"] = settings.Fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (settings.Limit is not null)
            query["limit"] = settings.Limit;
        if (settings.Offset is not null)
            query["offset"] = settings.Offset;
        if (settings is RequestCommandSettings withPackageIds && withPackageIds.PackageIds.Length > 0)
            query["packageIds"] = withPackageIds.PackageIds;

        return query;
    }
}

public abstract class FixedRequestPlanCommand<TSettings> : RequestPlanCommandBase<TSettings>
    where TSettings : DeviceCommandSettings, IRequestPlanSelectorSettings
{
    protected FixedRequestPlanCommand(
        IProfileResolver profileResolver,
        IOutputRenderer outputRenderer,
        IDiagnosticLogger diagnosticLogger,
        IMyJdTransport transport,
        IConfirmationGuard confirmationGuard)
        : base(profileResolver, outputRenderer, diagnosticLogger, transport, confirmationGuard)
    {
    }

    protected abstract string Operation { get; }
    protected abstract string Endpoint { get; }
    protected virtual string Method => "POST";
    protected virtual bool Destructive => false;
    protected virtual bool ProducesBinary => false;

    protected override MyJdRequestPlan CreatePlan(CommandContext context, TSettings settings, ResolvedProfileContext resolved)
    {
        return new MyJdRequestPlan(
            Operation,
            Method,
            Endpoint,
            BuildSelectorQuery(settings),
            null,
            Destructive,
            ProducesBinary,
            resolved.Device?.Id);
    }
}
