using System.Collections;
using System.Text.Json;
using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport;

internal static class MyJdParameterSupport
{
    public static (object? Parameters, IReadOnlyList<string>? Warnings) BuildJsonStringParameter(
        object queryObject,
        IReadOnlyList<string>? warnings)
    {
        return (new object?[] { JsonSerializer.Serialize(queryObject) }, warnings);
    }

    public static (object? Parameters, IReadOnlyList<string>? Warnings) BuildRawParameters(MyJdRequestPlan plan)
    {
        var queryIsEmpty = MyJdValueReaders.IsEmpty(plan.Query);
        var bodyIsEmpty = MyJdValueReaders.IsEmpty(plan.Body);
        if (queryIsEmpty && bodyIsEmpty)
            return (null, null);
        if (bodyIsEmpty)
            return (WrapSingleParameter(plan.Query), null);
        if (queryIsEmpty)
            return (WrapSingleParameter(plan.Body), null);

        return (new object?[] { plan.Query, plan.Body }, null);
    }

    public static (object? Parameters, IReadOnlyList<string>? Warnings) BuildGenericParameters(MyJdRequestPlan plan)
    {
        var queryIsEmpty = MyJdValueReaders.IsEmpty(plan.Query);
        var bodyIsEmpty = MyJdValueReaders.IsEmpty(plan.Body);
        if (queryIsEmpty && bodyIsEmpty)
            return (null, null);
        if (bodyIsEmpty)
            return (new object?[] { plan.Query }, null);
        if (queryIsEmpty)
            return (new object?[] { plan.Body }, null);

        return (new object?[] { plan.Query, plan.Body }, null);
    }

    private static object? WrapSingleParameter(object? value)
    {
        if (value is null)
            return null;
        if (value is IEnumerable sequence
            && value is not string
            && value is not Dictionary<string, object?>)
        {
            return sequence;
        }

        return new object?[] { value };
    }

    public static (object? Parameters, IReadOnlyList<string>? Warnings) EnsureNoParameters(MyJdRequestPlan plan, string usageMessage)
    {
        if (!MyJdValueReaders.IsEmpty(plan.Query) || !MyJdValueReaders.IsEmpty(plan.Body))
            throw CliException.Usage(usageMessage);

        return (null, null);
    }

    public static object? EnsureNoBody(MyJdRequestPlan plan, string usageMessage)
    {
        if (!MyJdValueReaders.IsEmpty(plan.Body))
            throw CliException.Usage(usageMessage);

        return plan.Query;
    }

    public static object? EnsureNoBodyForMappedEndpoint(MyJdRequestPlan plan)
    {
        return EnsureNoBody(plan, $"Endpoint '{plan.Endpoint}' does not accept --body-json.");
    }

    public static bool TryGetLong(Dictionary<string, object?> values, string[] keys, out long longValue)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var rawValue) && rawValue is not null && MyJdValueReaders.TryReadLong(rawValue, out longValue))
                return true;
        }

        longValue = default;
        return false;
    }

    public static bool TryGetString(Dictionary<string, object?> values, string[] keys, out string value)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var rawValue) && rawValue is not null && !string.IsNullOrWhiteSpace(rawValue.ToString()))
            {
                value = rawValue.ToString()!.Trim();
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    public static bool ReadOptionalBool(Dictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var rawValue) || rawValue is null)
            return false;
        if (MyJdValueReaders.TryReadBool(rawValue, out var boolValue))
            return boolValue;

        throw CliException.Usage($"Invalid boolean value for '{key}'.");
    }

    public static (object? Parameters, IReadOnlyList<string>? Warnings) BuildLongArrayParameters(
        object? query,
        string[] keys,
        string usageMessage,
        out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values)
        {
            foreach (var key in keys)
            {
                if (values.TryGetValue(key, out var rawValues) && MyJdValueReaders.TryReadLongArray(rawValues, out var longValues))
                    return (new object?[] { longValues }, null);
            }
        }

        throw CliException.Usage(usageMessage);
    }

    public static (object? Parameters, IReadOnlyList<string>? Warnings) BuildSingleStringParameter(
        object? query,
        string key,
        string usageMessage,
        out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue(key, out var rawValue)
            && rawValue is not null
            && !string.IsNullOrWhiteSpace(rawValue.ToString()))
        {
            return (new object?[] { rawValue.ToString() }, null);
        }

        throw CliException.Usage(usageMessage);
    }

    public static (object? Parameters, IReadOnlyList<string>? Warnings) BuildSingleLongParameter(
        object? query,
        string key,
        string usageMessage,
        out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue(key, out var rawValue)
            && rawValue is not null
            && MyJdValueReaders.TryReadLong(rawValue, out var longValue))
        {
            return (new object?[] { longValue }, null);
        }

        throw CliException.Usage(usageMessage);
    }

    public static (object? Parameters, IReadOnlyList<string>? Warnings) BuildSingleBooleanParameter(
        object? query,
        string key,
        string usageMessage,
        out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue(key, out var rawValue)
            && rawValue is not null
            && MyJdValueReaders.TryReadBool(rawValue, out var boolValue))
        {
            return (new object?[] { boolValue }, null);
        }

        throw CliException.Usage(usageMessage);
    }

    public static (object? Parameters, IReadOnlyList<string>? Warnings) BuildStringArrayParameters(
        object? query,
        string key,
        string usageMessage,
        out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue(key, out var rawValue))
        {
            var items = MyJdValueReaders.ToStringList(rawValue);
            if (items.Count > 0)
                return (new object?[] { items.ToArray() }, null);
        }

        throw CliException.Usage(usageMessage);
    }

    public static (object? Parameters, IReadOnlyList<string>? Warnings) BuildLinkAndPackageIdsParameters(
        object? query,
        string usageMessage,
        out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values)
            throw CliException.Usage(usageMessage);

        long[] linkIds = [];
        var hasLinkIds = values.TryGetValue("linkIds", out var rawLinkIds) && MyJdValueReaders.TryReadLongArray(rawLinkIds, out linkIds);

        long[] packageIds = [];
        var hasPackageIds = values.TryGetValue("packageIds", out var rawPackageIds) && MyJdValueReaders.TryReadLongArray(rawPackageIds, out packageIds);

        if (!hasLinkIds && !hasPackageIds)
            throw CliException.Usage(usageMessage);

        return (new object?[] { hasLinkIds ? linkIds : Array.Empty<long>(), hasPackageIds ? packageIds : Array.Empty<long>() }, null);
    }
}
