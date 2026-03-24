using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport;

internal static class MyJdAccountParameterBuilders
{
    public static bool TryBuild(MyJdRequestPlan plan, out (object? Parameters, IReadOnlyList<string>? Warnings) result)
    {
        switch (plan.Endpoint)
        {
            case "/accountsV2/disableAccounts":
                result = MyJdParameterSupport.BuildLongArrayParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), ["accountIds", "ids"], "accounts disable requires at least one --account-id <id>.", out _);
                return true;
            case "/accountsV2/enableAccounts":
                result = MyJdParameterSupport.BuildLongArrayParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), ["accountIds", "ids"], "accounts enable requires at least one --account-id <id>.", out _);
                return true;
            case "/accountsV2/refreshAccounts":
                result = MyJdParameterSupport.BuildLongArrayParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), ["accountIds", "ids"], "accounts refresh requires at least one --account-id <id>.", out _);
                return true;
            case "/accountsV2/removeAccounts":
                result = MyJdParameterSupport.BuildLongArrayParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), ["accountIds", "ids"], "accounts remove requires at least one --account-id <id>.", out _);
                return true;
            case "/accountsV2/removeBasicAuths":
                result = MyJdParameterSupport.BuildLongArrayParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), ["basicAuthIds", "ids"], "accounts basic-auth remove requires at least one --basic-auth-id <id>.", out _);
                return true;
            case "/accountsV2/addAccount":
                result = BuildAccountsAddParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/accountsV2/addBasicAuth":
                result = BuildBasicAuthAddParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/accountsV2/updateBasicAuth":
                result = BuildBasicAuthUpdateParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/accountsV2/setUserNameAndPassword":
                result = BuildAccountsUpdateParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/accountsV2/getPremiumHosterUrl":
                result = BuildAccountsGetParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/accountsV2/listBasicAuth":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "accounts basic-auth list does not accept query/body parameters.");
                return true;
            case "/accountsV2/listPremiumHoster":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "accounts hosters list does not accept query/body parameters.");
                return true;
            case "/accountsV2/listPremiumHosterUrls":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "accounts hosters urls does not accept query/body parameters.");
                return true;
            default:
                result = default;
                return false;
        }
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildAccountsGetParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("hoster", out var rawHoster)
            && rawHoster is not null
            && !string.IsNullOrWhiteSpace(rawHoster.ToString()))
        {
            return (new object?[] { rawHoster.ToString() }, null);
        }

        throw CliException.Usage("accounts get requires --hoster <name>.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildAccountsUpdateParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("accountId", out var rawAccountId)
            && values.TryGetValue("username", out var rawUsername)
            && values.TryGetValue("password", out var rawPassword)
            && rawAccountId is not null
            && rawUsername is not null
            && rawPassword is not null
            && MyJdValueReaders.TryReadLong(rawAccountId, out var accountId)
            && !string.IsNullOrWhiteSpace(rawUsername.ToString()))
        {
            return (new object?[] { accountId, rawUsername.ToString(), rawPassword.ToString() }, null);
        }

        throw CliException.Usage("accounts update requires --account-id <id> --username <name> and exactly one password source.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildAccountsAddParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("hoster", out var rawHoster)
            && values.TryGetValue("username", out var rawUsername)
            && values.TryGetValue("password", out var rawPassword)
            && rawHoster is not null
            && rawUsername is not null
            && rawPassword is not null
            && !string.IsNullOrWhiteSpace(rawHoster.ToString())
            && !string.IsNullOrWhiteSpace(rawUsername.ToString()))
        {
            return (new object?[] { rawHoster.ToString(), rawUsername.ToString(), rawPassword.ToString() }, null);
        }

        throw CliException.Usage("accounts add requires --hoster <name> --username <name> and exactly one password source.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildBasicAuthAddParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("type", out var rawType)
            && values.TryGetValue("hostmask", out var rawHostmask)
            && values.TryGetValue("username", out var rawUsername)
            && values.TryGetValue("password", out var rawPassword)
            && rawType is not null
            && rawHostmask is not null
            && rawUsername is not null
            && rawPassword is not null
            && !string.IsNullOrWhiteSpace(rawType.ToString())
            && !string.IsNullOrWhiteSpace(rawHostmask.ToString())
            && !string.IsNullOrWhiteSpace(rawUsername.ToString()))
        {
            return (new object?[] { rawType.ToString(), rawHostmask.ToString(), rawUsername.ToString(), rawPassword.ToString() }, null);
        }

        throw CliException.Usage("accounts basic-auth add requires --type <http|ftp> --hostmask <mask> --username <name> and exactly one password source.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildBasicAuthUpdateParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("id", out var rawId)
            && values.TryGetValue("type", out var rawType)
            && values.TryGetValue("hostmask", out var rawHostmask)
            && values.TryGetValue("username", out var rawUsername)
            && values.TryGetValue("password", out var rawPassword)
            && rawId is not null
            && rawType is not null
            && rawHostmask is not null
            && rawUsername is not null
            && rawPassword is not null
            && MyJdValueReaders.TryReadLong(rawId, out var id)
            && !string.IsNullOrWhiteSpace(rawType.ToString())
            && !string.IsNullOrWhiteSpace(rawHostmask.ToString())
            && !string.IsNullOrWhiteSpace(rawUsername.ToString()))
        {
            var updatedEntry = new Dictionary<string, object?>
            {
                ["id"] = id,
                ["type"] = rawType.ToString(),
                ["hostmask"] = rawHostmask.ToString(),
                ["username"] = rawUsername.ToString(),
                ["password"] = rawPassword.ToString(),
            };
            return (new object?[] { updatedEntry }, null);
        }

        throw CliException.Usage("accounts basic-auth update requires --basic-auth-id <id> --type <http|ftp> --hostmask <mask> --username <name> and exactly one password source.");
    }
}
