using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport;

internal static class MyJdQueryParameterBuilders
{
    private static readonly string[] GrabberLinkFields =
    [
        "availability",
        "bytesTotal",
        "comment",
        "enabled",
        "host",
        "password",
        "priority",
        "status",
        "url",
        "variantID",
        "variantIcon",
        "variantName",
        "variants",
    ];

    public static bool TryBuild(MyJdRequestPlan plan, out (object? Parameters, IReadOnlyList<string>? Warnings) result)
    {
        switch (plan.Endpoint)
        {
            case "/linkgrabberv2/queryLinks":
                result = MyJdParameterSupport.BuildJsonStringParameter(
                    BuildGrabberLinksQuery(MyJdParameterSupport.EnsureNoBody(plan, "grabber links list does not accept --body-json."), out var warnings),
                    warnings);
                return true;
            case "/linkgrabberv2/queryPackages":
                result = MyJdParameterSupport.BuildJsonStringParameter(
                    BuildGrabberPackagesQuery(MyJdParameterSupport.EnsureNoBody(plan, "grabber packages list does not accept --body-json."), out warnings),
                    warnings);
                return true;
            case "/linkgrabberv2/queryLinkCrawlerJobs":
                result = MyJdParameterSupport.BuildJsonStringParameter(
                    BuildGrabberJobsQuery(MyJdParameterSupport.EnsureNoBody(plan, "grabber jobs list does not accept --body-json."), out warnings),
                    warnings);
                return true;
            case "/downloadsV2/queryLinks":
                result = MyJdParameterSupport.BuildJsonStringParameter(
                    BuildDownloadsLinksQuery(MyJdParameterSupport.EnsureNoBody(plan, "downloads links list does not accept --body-json."), out warnings),
                    warnings);
                return true;
            case "/downloadsV2/queryPackages":
                result = MyJdParameterSupport.BuildJsonStringParameter(
                    BuildDownloadsPackagesQuery(MyJdParameterSupport.EnsureNoBody(plan, "downloads packages list does not accept --body-json."), out warnings),
                    warnings);
                return true;
            case "/extensions/list":
                result = MyJdParameterSupport.BuildJsonStringParameter(
                    BuildExtensionsQuery(MyJdParameterSupport.EnsureNoBody(plan, "settings extensions list does not accept --body-json."), out warnings),
                    warnings);
                return true;
            case "/plugins/list":
                result = MyJdParameterSupport.BuildJsonStringParameter(
                    BuildPluginsQuery(MyJdParameterSupport.EnsureNoBody(plan, "settings plugins list does not accept --body-json."), out warnings),
                    warnings);
                return true;
            case "/accountsV2/listAccounts":
                result = MyJdParameterSupport.BuildJsonStringParameter(
                    BuildAccountsQuery(MyJdParameterSupport.EnsureNoBody(plan, "accounts list does not accept --body-json."), out warnings),
                    warnings);
                return true;
            default:
                result = default;
                return false;
        }
    }

    private static object BuildGrabberLinksQuery(object? query, out IReadOnlyList<string>? warnings)
    {
        var projection = CreateProjection(GrabberLinkFields, includeByDefault: true);
        projection["maxResults"] = -1;
        projection["startAt"] = 0;
        return BuildQueryObject(query, projection, out warnings, "packageUUIDs");
    }

    private static object BuildDownloadsLinksQuery(object? query, out IReadOnlyList<string>? warnings)
    {
        var projection = CreateProjection(
            ["addedDate", "bytesLoaded", "bytesTotal", "comment", "enabled", "eta", "extractionStatus", "finished", "finishedDate", "host", "password", "priority", "running", "saveTo", "skipped", "speed", "status", "url"],
            includeByDefault: true);
        projection["maxResults"] = 20;
        projection["startAt"] = 0;
        return BuildQueryObject(query, projection, out warnings, "packageUUIDs", "jobUUIDs");
    }

    private static object BuildGrabberPackagesQuery(object? query, out IReadOnlyList<string>? warnings)
    {
        var projection = CreateProjection(
            ["availableOfflineCount", "availableOnlineCount", "availableTempUnknownCount", "availableUnknownCount", "bytesTotal", "childCount", "comment", "enabled", "hosts", "priority", "saveTo", "status"],
            includeByDefault: true);
        projection["maxResults"] = -1;
        projection["startAt"] = 0;
        return BuildQueryObject(query, projection, out warnings, "packageUUIDs");
    }

    private static object BuildDownloadsPackagesQuery(object? query, out IReadOnlyList<string>? warnings)
    {
        var projection = CreateProjection(
            ["bytesLoaded", "bytesTotal", "childCount", "comment", "enabled", "eta", "finished", "hosts", "priority", "running", "saveTo", "speed", "status"],
            includeByDefault: true);
        projection["maxResults"] = 20;
        projection["startAt"] = 0;
        return BuildQueryObject(query, projection, out warnings, "packageUUIDs");
    }

    private static object BuildGrabberJobsQuery(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values || values.Count == 0)
            return new Dictionary<string, object?> { ["collectorInfo"] = true };
        if (values.TryGetValue("queryOverride", out var queryOverride) && queryOverride is not null)
            return queryOverride;
        if (!IsSelectorQuery(values))
            return values;

        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["collectorInfo"] = true };
        var localWarnings = new List<string>();

        if (values.TryGetValue("limit", out var limit) && MyJdValueReaders.TryReadInt(limit, out var maxResults))
            result["maxResults"] = maxResults;
        if (values.TryGetValue("offset", out var offset) && MyJdValueReaders.TryReadInt(offset, out var startAt))
            result["startAt"] = startAt;

        if (values.TryGetValue("jobIds", out var jobIds) && MyJdValueReaders.TryReadLongArray(jobIds, out var parsedJobIds))
        {
            result["jobIds"] = parsedJobIds;
        }
        else if (values.TryGetValue("packageIds", out var legacyJobIds) && MyJdValueReaders.TryReadLongArray(legacyJobIds, out parsedJobIds))
        {
            result["jobIds"] = parsedJobIds;
            localWarnings.Add("Selector key 'packageIds' is deprecated for grabber jobs list. Use 'jobIds'.");
        }

        if (values.TryGetValue("fields", out var fields) && MyJdValueReaders.ToStringList(fields).Count > 0)
            throw CliException.Usage("grabber jobs list does not support --fields.");
        if (values.TryGetValue("linkIds", out var linkIds) && !MyJdValueReaders.IsEmpty(linkIds))
            throw CliException.Usage("grabber jobs list does not support --link-id.");
        if (values.TryGetValue("hosters", out var hosters) && !MyJdValueReaders.IsEmpty(hosters))
            throw CliException.Usage("grabber jobs list does not support --hoster.");

        warnings = localWarnings.Count == 0 ? null : localWarnings;
        return result;
    }

    private static object BuildExtensionsQuery(object? query, out IReadOnlyList<string>? warnings)
    {
        var projection = CreateProjection(["configInterface", "description", "enabled", "iconKey", "id", "installed", "name"], includeByDefault: true);
        projection["pattern"] = string.Empty;
        return BuildQueryObject(query, projection, out warnings);
    }

    private static object BuildPluginsQuery(object? query, out IReadOnlyList<string>? warnings)
    {
        return BuildQueryObject(query, CreateProjection(["pattern", "version"], includeByDefault: true), out warnings);
    }

    private static object BuildAccountsQuery(object? query, out IReadOnlyList<string>? warnings)
    {
        var projection = CreateProjection(["enabled", "error", "trafficLeft", "trafficMax", "userName", "valid", "validUntil"], includeByDefault: true);
        projection["maxResults"] = 20;
        projection["startAt"] = 0;
        return BuildQueryObject(query, projection, out warnings, "uuidlist");
    }

    private static object BuildQueryObject(
        object? query,
        Dictionary<string, object?> defaults,
        out IReadOnlyList<string>? warnings,
        params string[] longArrayFields)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values || values.Count == 0)
            return defaults;
        if (values.TryGetValue("queryOverride", out var queryOverride) && queryOverride is not null)
            return queryOverride;
        if (!IsSelectorQuery(values))
            return values;

        var result = new Dictionary<string, object?>(defaults, StringComparer.OrdinalIgnoreCase);
        var localWarnings = new List<string>();

        if (values.TryGetValue("limit", out var limit) && MyJdValueReaders.TryReadInt(limit, out var maxResults))
            result["maxResults"] = maxResults;
        if (values.TryGetValue("offset", out var offset) && MyJdValueReaders.TryReadInt(offset, out var startAt))
            result["startAt"] = startAt;
        if (values.TryGetValue("fields", out var fields))
            ApplyProjectionFields(result, fields, localWarnings);

        foreach (var fieldName in longArrayFields)
        {
            var selectorKey = fieldName.Equals("packageUUIDs", StringComparison.OrdinalIgnoreCase) ? "packageIds" : fieldName;
            if (values.TryGetValue(selectorKey, out var rawValues) && MyJdValueReaders.TryReadLongArray(rawValues, out var longValues))
                result[fieldName] = longValues;
        }

        if (values.TryGetValue("packageIds", out var packageIds) && !MyJdValueReaders.IsEmpty(packageIds) && !longArrayFields.Contains("packageUUIDs", StringComparer.OrdinalIgnoreCase))
            throw CliException.Usage("This endpoint does not support --package-id.");
        if (values.TryGetValue("hosters", out var hosters) && !MyJdValueReaders.IsEmpty(hosters))
            throw CliException.Usage("This endpoint does not support --hoster.");
        if (values.TryGetValue("linkIds", out var linkIds) && !MyJdValueReaders.IsEmpty(linkIds))
            throw CliException.Usage("This endpoint does not support --link-id.");

        warnings = localWarnings.Count == 0 ? null : localWarnings;
        return result;
    }

    private static bool IsSelectorQuery(Dictionary<string, object?> values)
    {
        foreach (var key in values.Keys)
        {
            if (string.Equals(key, "fields", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "limit", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "offset", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "linkIds", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "packageIds", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "hosters", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "queryOverride", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static Dictionary<string, object?> CreateProjection(IEnumerable<string> fieldNames, bool includeByDefault)
    {
        var projection = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var fieldName in fieldNames)
            projection[fieldName] = includeByDefault;
        return projection;
    }

    private static void ApplyProjectionFields(Dictionary<string, object?> target, object? rawFields, List<string> warnings)
    {
        var requestedFields = MyJdValueReaders.ToStringList(rawFields);
        if (requestedFields.Count == 0)
            return;

        var projectionKeys = target.Keys.Where(key => target[key] is bool).ToList();
        foreach (var key in projectionKeys)
            target[key] = false;

        foreach (var field in requestedFields)
        {
            var key = projectionKeys.FirstOrDefault(candidate => string.Equals(candidate, field, StringComparison.OrdinalIgnoreCase))
                ?? projectionKeys.FirstOrDefault(candidate => string.Equals(candidate, NormalizeFieldAlias(field), StringComparison.OrdinalIgnoreCase));
            if (key is null)
            {
                warnings.Add($"Unknown projection field '{field}' was ignored.");
                continue;
            }

            target[key] = true;
        }
    }

    private static string NormalizeFieldAlias(string field)
    {
        return field switch
        {
            "variantId" => "variantID",
            "jobUuids" => "jobUUIDs",
            "packageUuids" => "packageUUIDs",
            _ => field,
        };
    }
}
