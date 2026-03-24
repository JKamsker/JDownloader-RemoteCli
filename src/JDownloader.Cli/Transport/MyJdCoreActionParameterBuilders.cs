using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport;

internal static class MyJdCoreActionParameterBuilders
{
    public static bool TryBuild(MyJdRequestPlan plan, out (object? Parameters, IReadOnlyList<string>? Warnings) result)
    {
        switch (plan.Endpoint)
        {
            case "/device/ping":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "device ping does not accept query/body parameters.");
                return true;
            case "/device/getDirectConnectionInfos":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "device direct-info does not accept query/body parameters.");
                return true;
            case "/downloadcontroller/getCurrentState":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "downloads status does not accept query/body parameters.");
                return true;
            case "/downloadcontroller/getSpeedInBps":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "downloads speed does not accept query/body parameters.");
                return true;
            case "/downloadcontroller/start":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "downloads start does not accept query/body parameters.");
                return true;
            case "/downloadcontroller/stop":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "downloads stop does not accept query/body parameters.");
                return true;
            case "/linkgrabberv2/clearList":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "grabber clear does not accept query/body parameters.");
                return true;
            case "/extraction/addArchivePassword":
                result = MyJdParameterSupport.BuildSingleStringParameter(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "password", "extraction add-password requires exactly one of --password <password> or --password-stdin.", out _);
                return true;
            case "/extraction/startExtractionNow":
                result = MyJdParameterSupport.BuildLinkAndPackageIdsParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "extraction start requires at least one --link-id <id> or --package-id <id>.", out _);
                return true;
            case "/extraction/cancelExtraction":
                result = MyJdParameterSupport.BuildSingleLongParameter(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "controllerId", "extraction cancel requires --controller-id <id>.", out _);
                return true;
            case "/extraction/getArchiveSettings":
                result = MyJdParameterSupport.BuildStringArrayParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "archiveIds", "extraction settings get requires at least one --archive-id <id>.", out _);
                return true;
            case "/extraction/setArchiveSettings":
                result = BuildExtractionSetArchiveSettingsParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/extraction/getArchiveInfo":
                result = MyJdParameterSupport.BuildLinkAndPackageIdsParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "extraction info requires at least one --link-id <id> or --package-id <id>.", out _);
                return true;
            case "/downloadsV2/removeLinks":
                result = MyJdParameterSupport.BuildLinkAndPackageIdsParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "downloads links remove requires at least one --link-id <id> or --package-id <id>.", out _);
                return true;
            case "/linkgrabberv2/moveToDownloadlist":
                result = MyJdParameterSupport.BuildLinkAndPackageIdsParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "grabber move-to-downloads requires at least one --link-id <id> or --package-id <id>.", out _);
                return true;
            case "/linkgrabberv2/removeLinks":
                result = MyJdParameterSupport.BuildLinkAndPackageIdsParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "grabber links remove requires at least one --link-id <id> or --package-id <id>.", out _);
                return true;
            case "/linkgrabberv2/getVariants":
                result = MyJdParameterSupport.BuildSingleLongParameter(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "linkId", "grabber variants list requires --link-id <id>.", out _);
                return true;
            case "/linkgrabberv2/setVariant":
                result = BuildGrabberSetVariantParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/downloadsV2/setStopMark":
                result = BuildDownloadsStopMarkParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/downloadsV2/removeStopMark":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "downloads stopmark clear does not accept query/body parameters.");
                return true;
            case "/downloadcontroller/pause":
                result = MyJdParameterSupport.BuildSingleBooleanParameter(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "value", "downloads pause requires either no flags (pause) or --resume.", out _);
                return true;
            case "/downloadsV2/getStopMark":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "downloads stopmark get does not accept query/body parameters.");
                return true;
            case "/config/get":
                result = BuildConfigParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "settings config get requires --interface-name <name> --key <key>.", out _);
                return true;
            case "/config/list":
                result = BuildConfigListParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/config/reset":
                result = BuildConfigParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "settings config reset requires --interface-name <name> --key <key>.", out _);
                return true;
            case "/config/set":
                result = BuildConfigSetParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/extraction/getQueue":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "extraction queue does not accept query/body parameters.");
                return true;
            default:
                result = default;
                return false;
        }
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildDownloadsStopMarkParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values)
            throw CliException.Usage("downloads stopmark set requires --link-id <id> --package-id <id>.");

        long? linkId = null;
        if (values.TryGetValue("linkId", out var rawLinkId) && rawLinkId is not null && MyJdValueReaders.TryReadLong(rawLinkId, out var parsedLinkId))
            linkId = parsedLinkId;

        long? packageId = null;
        if (values.TryGetValue("packageId", out var rawPackageId) && rawPackageId is not null && MyJdValueReaders.TryReadLong(rawPackageId, out var parsedPackageId))
            packageId = parsedPackageId;

        if (linkId is null || packageId is null)
            throw CliException.Usage("downloads stopmark set requires --link-id <id> --package-id <id>.");

        return (new object?[] { linkId.Value, packageId.Value }, null);
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildGrabberSetVariantParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("linkId", out var rawLinkId)
            && values.TryGetValue("variantId", out var rawVariantId)
            && rawLinkId is not null
            && rawVariantId is not null
            && MyJdValueReaders.TryReadLong(rawLinkId, out var linkId)
            && !string.IsNullOrWhiteSpace(rawVariantId.ToString()))
        {
            return (new object?[] { linkId, rawVariantId.ToString() }, null);
        }

        throw CliException.Usage("grabber variants set requires --link-id <id> --variant-id <id>.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildConfigParameters(object? query, string usageMessage, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("interfaceName", out var rawInterfaceName)
            && values.TryGetValue("key", out var rawKey)
            && rawInterfaceName is not null
            && rawKey is not null
            && !string.IsNullOrWhiteSpace(rawInterfaceName.ToString())
            && !string.IsNullOrWhiteSpace(rawKey.ToString()))
        {
            var storage = values.TryGetValue("storage", out var rawStorage) ? rawStorage?.ToString() ?? string.Empty : string.Empty;
            return (new object?[] { rawInterfaceName.ToString(), storage, rawKey.ToString() }, null);
        }

        throw CliException.Usage(usageMessage);
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildConfigSetParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("interfaceName", out var rawInterfaceName)
            && values.TryGetValue("key", out var rawKey)
            && values.TryGetValue("value", out var rawValue)
            && rawInterfaceName is not null
            && rawKey is not null
            && !string.IsNullOrWhiteSpace(rawInterfaceName.ToString())
            && !string.IsNullOrWhiteSpace(rawKey.ToString()))
        {
            var storage = values.TryGetValue("storage", out var rawStorage) ? rawStorage?.ToString() ?? string.Empty : string.Empty;
            return (new object?[] { rawInterfaceName.ToString(), storage, rawKey.ToString(), rawValue }, null);
        }

        throw CliException.Usage("settings config set requires --interface-name <name> --key <key> and exactly one of --value <value> or --value-json <json>.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildConfigListParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is null)
            return (null, null);
        if (query is not Dictionary<string, object?> values || values.Count == 0)
            return (null, null);

        var pattern = values.TryGetValue("pattern", out var rawPattern) ? rawPattern?.ToString() ?? string.Empty : string.Empty;
        var returnDescription = MyJdParameterSupport.ReadOptionalBool(values, "returnDescription");
        var returnValues = MyJdParameterSupport.ReadOptionalBool(values, "returnValues");
        var returnDefaultValues = MyJdParameterSupport.ReadOptionalBool(values, "returnDefaultValues");
        var returnEnumInfo = MyJdParameterSupport.ReadOptionalBool(values, "returnEnumInfo");

        return (new object?[] { pattern, returnDescription, returnValues, returnDefaultValues, returnEnumInfo }, null);
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildExtractionSetArchiveSettingsParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("archiveId", out var rawArchiveId)
            && rawArchiveId is not null
            && !string.IsNullOrWhiteSpace(rawArchiveId.ToString())
            && values.TryGetValue("archiveSettings", out var rawArchiveSettings)
            && rawArchiveSettings is not null)
        {
            return (new object?[] { rawArchiveId.ToString(), rawArchiveSettings }, null);
        }

        throw CliException.Usage("extraction settings set requires --archive-id <id> --settings-json <json-or-@file>.");
    }
}
