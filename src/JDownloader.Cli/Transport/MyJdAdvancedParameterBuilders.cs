using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport;

internal static class MyJdAdvancedParameterBuilders
{
    public static bool TryBuild(MyJdRequestPlan plan, out (object? Parameters, IReadOnlyList<string>? Warnings) result)
    {
        switch (plan.Endpoint)
        {
            case "/contentV2/getIcon":
                result = BuildContentGetIconParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/contentV2/getFavIcon":
                result = BuildContentGetFavIconParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/contentV2/getFileIcon":
                result = BuildContentGetFileIconParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/contentV2/getIconDescription":
                result = BuildContentGetIconDescriptionParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/dialogs/get":
                result = BuildDialogsGetParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/dialogs/list":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "advanced dialogs list does not accept query/body parameters.");
                return true;
            case "/dialogs/getTypeInfo":
                result = MyJdParameterSupport.BuildSingleStringParameter(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "dialogType", "advanced dialogs type-info requires --dialog-type <type>.", out _);
                return true;
            case "/dialogs/answer":
                result = BuildDialogsAnswerParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/flash/add":
                result = BuildFlashAddParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/linkgrabberv2/addContainer":
                result = BuildGrabberAddContainerParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/linkgrabberv2/addLinks":
                result = BuildGrabberAddLinksParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            default:
                result = default;
                return false;
        }
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildContentGetIconParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("key", out var rawKey)
            && values.TryGetValue("size", out var rawSize)
            && rawKey is not null
            && rawSize is not null
            && !string.IsNullOrWhiteSpace(rawKey.ToString())
            && MyJdValueReaders.TryReadInt(rawSize, out var size)
            && size > 0)
        {
            return (new object?[] { rawKey.ToString(), size }, null);
        }

        throw CliException.Usage("advanced content icon requires --key <key> and --size <px>.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildContentGetFavIconParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("hostername", out var rawHostername)
            && rawHostername is not null
            && !string.IsNullOrWhiteSpace(rawHostername.ToString()))
        {
            return (new object?[] { rawHostername.ToString() }, null);
        }

        throw CliException.Usage("advanced content favicon requires --hoster <name>.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildContentGetFileIconParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("filename", out var rawFilename)
            && rawFilename is not null
            && !string.IsNullOrWhiteSpace(rawFilename.ToString()))
        {
            return (new object?[] { rawFilename.ToString() }, null);
        }

        throw CliException.Usage("advanced content file-icon requires --filename <name>.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildContentGetIconDescriptionParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("key", out var rawKey)
            && rawKey is not null
            && !string.IsNullOrWhiteSpace(rawKey.ToString()))
        {
            return (new object?[] { rawKey.ToString() }, null);
        }

        throw CliException.Usage("advanced content describe requires --key <key>.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildDialogsGetParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("id", out var rawId)
            && values.TryGetValue("icon", out var rawIcon)
            && values.TryGetValue("properties", out var rawProperties)
            && rawId is not null
            && rawIcon is not null
            && rawProperties is not null
            && MyJdValueReaders.TryReadLong(rawId, out var id)
            && MyJdValueReaders.TryReadBool(rawIcon, out var icon)
            && MyJdValueReaders.TryReadBool(rawProperties, out var properties))
        {
            return (new object?[] { id, icon, properties }, null);
        }

        throw CliException.Usage("advanced dialogs get requires --id <id>.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildDialogsAnswerParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("id", out var rawId)
            && values.TryGetValue("data", out var rawData)
            && rawId is not null
            && rawData is not null
            && MyJdValueReaders.TryReadLong(rawId, out var id)
            && rawData is Dictionary<string, object?>)
        {
            return (new object?[] { id, rawData }, null);
        }

        throw CliException.Usage("advanced dialogs answer requires --id <id> and --data-json <json-object-or-@file>.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildFlashAddParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("url", out var rawUrl)
            && values.TryGetValue("source", out var rawSource)
            && values.TryGetValue("password", out var rawPassword)
            && rawUrl is not null
            && rawSource is not null
            && rawPassword is not null
            && !string.IsNullOrWhiteSpace(rawUrl.ToString())
            && !string.IsNullOrWhiteSpace(rawSource.ToString()))
        {
            return (new object?[] { rawPassword.ToString() ?? string.Empty, rawSource.ToString(), rawUrl.ToString() }, null);
        }

        throw CliException.Usage("advanced ingest cnl requires --url <url>.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildGrabberAddContainerParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("type", out var rawType)
            && values.TryGetValue("content", out var rawContent)
            && rawType is not null
            && rawContent is not null
            && !string.IsNullOrWhiteSpace(rawType.ToString())
            && !string.IsNullOrWhiteSpace(rawContent.ToString()))
        {
            return (new object?[] { rawType.ToString(), rawContent.ToString() }, null);
        }

        throw CliException.Usage("grabber add-container requires --type <type> and --content <content>.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildGrabberAddLinksParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values)
        {
            var hasLinks = values.TryGetValue("links", out var links)
                && links is not null
                && !string.IsNullOrWhiteSpace(links.ToString());
            var hasDataUrls = values.TryGetValue("dataURLs", out var dataUrls)
                && dataUrls is not null
                && !MyJdValueReaders.IsEmpty(dataUrls);

            if (hasLinks || hasDataUrls)
                return (new object?[] { values }, null);
        }

        throw CliException.Usage("grabber add requires at least one link input (AddLinksQuery.links or AddLinksQuery.dataURLs).");
    }
}
