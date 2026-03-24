using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport;

internal static class MyJdCaptchaEventsParameterBuilders
{
    public static bool TryBuild(MyJdRequestPlan plan, out (object? Parameters, IReadOnlyList<string>? Warnings) result)
    {
        switch (plan.Endpoint)
        {
            case "/captcha/get":
                result = BuildCaptchaGetParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/captcha/getCaptchaJob":
                result = BuildCaptchaJobParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/captcha/list":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "captcha list does not accept query/body parameters.");
                return true;
            case "/captcha/skip":
                result = BuildCaptchaSkipParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/captcha/solve":
                result = BuildCaptchaSolveParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/captchaforward/createJobRecaptchaV2":
                result = BuildCaptchaForwardCreateJobParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/captchaforward/getResult":
                result = BuildCaptchaForwardGetResultParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/events/listen":
                result = BuildEventsListenParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/events/getsubscriptionstatus":
                result = BuildEventsSubscriptionStatusParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/events/listpublisher":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "events publishers does not accept query/body parameters.");
                return true;
            case "/events/subscribe":
                result = BuildEventsSubscribeParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/events/setsubscription":
                result = BuildEventsSetSubscriptionParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/events/removesubscription":
                result = BuildEventsRemoveSubscriptionParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            default:
                result = default;
                return false;
        }
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildCaptchaGetParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values || !MyJdParameterSupport.TryGetLong(values, ["id"], out var id))
            throw CliException.Usage("captcha get requires --id <id>.");

        var format = values.TryGetValue("format", out var rawFormat) ? rawFormat?.ToString() : null;
        return string.IsNullOrWhiteSpace(format) ? (new object?[] { id }, null) : (new object?[] { id, format }, null);
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildCaptchaJobParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values || !MyJdParameterSupport.TryGetLong(values, ["id"], out var id))
            throw CliException.Usage("captcha job requires --id <id>.");

        return (new object?[] { id }, null);
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildCaptchaSolveParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values || !MyJdParameterSupport.TryGetLong(values, ["id"], out var id))
            throw CliException.Usage("captcha solve requires --id <id> --result <text>.");
        if (!values.TryGetValue("result", out var rawResult) || rawResult is null || string.IsNullOrWhiteSpace(rawResult.ToString()))
            throw CliException.Usage("captcha solve requires --id <id> --result <text>.");

        var result = rawResult.ToString();
        var resultFormat = values.TryGetValue("resultFormat", out var rawResultFormat) ? rawResultFormat?.ToString() : null;
        return string.IsNullOrWhiteSpace(resultFormat) ? (new object?[] { id, result }, null) : (new object?[] { id, result, resultFormat }, null);
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildCaptchaSkipParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values || !MyJdParameterSupport.TryGetLong(values, ["id"], out var id))
            throw CliException.Usage("captcha skip requires --id <id>.");

        var type = values.TryGetValue("type", out var rawType) ? rawType?.ToString() : null;
        if (string.IsNullOrWhiteSpace(type))
        {
            warnings = ["captcha skip without --type uses a deprecated API overload. Prefer providing --type."];
            return (new object?[] { id }, warnings);
        }

        return (new object?[] { id, type }, null);
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildCaptchaForwardGetResultParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values || !MyJdParameterSupport.TryGetLong(values, ["id", "jobId", "jobid"], out var id))
            throw CliException.Usage("captcha forward get-result requires --job-id <id>.");

        return (new object?[] { id }, null);
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildCaptchaForwardCreateJobParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values
            || !MyJdParameterSupport.TryGetString(values, ["arg1"], out var arg1)
            || !MyJdParameterSupport.TryGetString(values, ["arg2"], out var arg2)
            || !MyJdParameterSupport.TryGetString(values, ["arg3"], out var arg3)
            || !MyJdParameterSupport.TryGetString(values, ["arg4"], out var arg4))
        {
            throw CliException.Usage("captcha forward create-job requires 4 arguments.");
        }

        return (new object?[] { arg1, arg2, arg3, arg4 }, null);
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildEventsListenParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values || !MyJdParameterSupport.TryGetLong(values, ["subscriptionid", "subscriptionId"], out var id))
            throw CliException.Usage("events listen requires --subscription-id <id>.");

        return (new object?[] { id }, null);
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildEventsSubscriptionStatusParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values || !MyJdParameterSupport.TryGetLong(values, ["subscriptionid", "subscriptionId"], out var id))
            throw CliException.Usage("events status requires --subscription-id <id>.");

        return (new object?[] { id }, null);
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildEventsSubscribeParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values)
            throw CliException.Usage("events subscribe requires at least one --subscription <name>.");

        var subscriptions = values.TryGetValue("subscriptions", out var rawSubscriptions) ? MyJdValueReaders.ToStringList(rawSubscriptions) : [];
        var exclusions = values.TryGetValue("exclusions", out var rawExclusions) ? MyJdValueReaders.ToStringList(rawExclusions) : [];
        if (subscriptions.Count == 0)
            throw CliException.Usage("events subscribe requires at least one --subscription <name>.");

        return (new object?[] { subscriptions.ToArray(), exclusions.ToArray() }, null);
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildEventsSetSubscriptionParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values || !MyJdParameterSupport.TryGetLong(values, ["subscriptionid", "subscriptionId"], out var id))
            throw CliException.Usage("events set requires --subscription-id <id>.");

        var subscriptions = values.TryGetValue("subscriptions", out var rawSubscriptions) ? MyJdValueReaders.ToStringList(rawSubscriptions) : [];
        var exclusions = values.TryGetValue("exclusions", out var rawExclusions) ? MyJdValueReaders.ToStringList(rawExclusions) : [];
        return (new object?[] { id, subscriptions.ToArray(), exclusions.ToArray() }, null);
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildEventsRemoveSubscriptionParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is not Dictionary<string, object?> values || !MyJdParameterSupport.TryGetLong(values, ["subscriptionid", "subscriptionId"], out var id))
            throw CliException.Usage("events remove requires --subscription-id <id>.");

        var subscriptions = values.TryGetValue("subscriptions", out var rawSubscriptions) ? MyJdValueReaders.ToStringList(rawSubscriptions) : [];
        var exclusions = values.TryGetValue("exclusions", out var rawExclusions) ? MyJdValueReaders.ToStringList(rawExclusions) : [];
        return (new object?[] { id, subscriptions.ToArray(), exclusions.ToArray() }, null);
    }
}
