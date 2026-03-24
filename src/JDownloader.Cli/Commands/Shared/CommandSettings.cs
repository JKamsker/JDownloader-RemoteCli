using System.ComponentModel;
using JDownloader.Cli.Runtime;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Shared;

public abstract class DeviceCommandSettings : GlobalSettings
{
}

public sealed class DeviceNoArgSettings : DeviceCommandSettings
{
}

public sealed class NoArgSettings : GlobalSettings
{
}

public interface IRequestPlanSelectorSettings
{
    string? Fields { get; }
    int? Limit { get; }
    int? Offset { get; }
    string? QueryJson { get; }
}

public class RequestCommandSettings : DeviceCommandSettings, IRequestPlanSelectorSettings
{
    [CommandOption("--fields <CSV>")]
    [Description("Comma-separated field projection for query-style endpoints.")]
    public string? Fields { get; init; }

    [CommandOption("--limit <NUMBER>")]
    [Description("Maximum number of results.")]
    public int? Limit { get; init; }

    [CommandOption("--offset <NUMBER>")]
    [Description("Result offset.")]
    public int? Offset { get; init; }

    [CommandOption("--package-id <ID>")]
    [Description("Repeatable package identifier filter.")]
    public string[] PackageIds { get; init; } = [];

    [CommandOption("--query-json <JSON>")]
    [Description("Raw query object JSON or @file override.")]
    public string? QueryJson { get; init; }
}

public class RequestCommandSettingsNoPackage : DeviceCommandSettings, IRequestPlanSelectorSettings
{
    [CommandOption("--fields <CSV>")]
    [Description("Comma-separated field projection for query-style endpoints.")]
    public string? Fields { get; init; }

    [CommandOption("--limit <NUMBER>")]
    [Description("Maximum number of results.")]
    public int? Limit { get; init; }

    [CommandOption("--offset <NUMBER>")]
    [Description("Result offset.")]
    public int? Offset { get; init; }

    [CommandOption("--query-json <JSON>")]
    [Description("Raw query object JSON or @file override.")]
    public string? QueryJson { get; init; }
}
