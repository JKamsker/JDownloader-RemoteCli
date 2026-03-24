using Spectre.Console.Cli;

namespace JDownloader.Cli.Documentation;

internal static class DocumentationExampleConventions
{
    private static readonly Dictionary<string, string[]> ManualExamples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["accounts add"] = ["accounts", "add", "--hoster", "ddownload.com", "--username", "demo", "--password-stdin"],
        ["accounts basic-auth add"] = ["accounts", "basic-auth", "add", "--type", "http", "--hostmask", "example.com", "--username", "demo", "--password-stdin"],
        ["advanced ingest cnl"] = ["advanced", "ingest", "cnl", "--urls", "https://example.invalid/file"],
        ["advanced raw request"] = ["advanced", "raw", "request", "/downloadsV2/queryLinks", "--query-json", "{}"],
        ["auth login"] = ["auth", "login", "--email", "demo@example.com", "--password-stdin"],
        ["captcha solve"] = ["captcha", "solve", "--id", "12345", "--result", "secret"],
        ["device use"] = ["device", "use", "--device", "jd-main"],
        ["downloads stopmark set"] = ["downloads", "stopmark", "set", "--link-id", "12345", "--package-id", "67890"],
        ["events set"] = ["events", "set", "--subscription-id", "12345", "--subscription", "linkgrabber"],
        ["events subscribe"] = ["events", "subscribe", "--subscription", "linkgrabber"],
        ["grabber add"] = ["grabber", "add", "--url", "https://example.invalid/file"],
        ["grabber add-container"] = ["grabber", "add-container", "--type", "dlc", "--content-file", "./sample.dlc"],
        ["settings config set"] = ["settings", "config", "set", "--interface-name", "GeneralSettings", "--key", "downloadFolder", "--value", "/downloads"],
        ["system toggle"] = ["system", "toggle", "pause-downloads"],
    };

    private static readonly HashSet<string> MutationCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "add",
        "answer",
        "approve",
        "cancel",
        "clear",
        "cnl",
        "create-job",
        "disable",
        "enable",
        "exit",
        "hibernate",
        "install",
        "listen",
        "login",
        "move-to-downloads",
        "pause",
        "refresh",
        "remove",
        "rename",
        "request",
        "restart",
        "run",
        "set",
        "shutdown",
        "skip",
        "solve",
        "start",
        "stop",
        "subscribe",
        "toggle",
        "update",
        "use",
    };

    public static bool TryGetManualExample(string commandPath, out string[] example)
    {
        return ManualExamples.TryGetValue(commandPath, out example!);
    }

    public static bool ShouldAddIllustrativeOption(string commandName, int argumentCount, IEnumerable<DocumentationExampleOption> options)
    {
        var optionalOptions = options.Any(option => !option.Attribute.IsRequired && !IsNoiseOption(option.Attribute));
        return optionalOptions && (argumentCount == 0 || MutationCommands.Contains(commandName));
    }

    public static bool IsNoiseOption(CommandOptionAttribute option)
    {
        var primaryName = NormalizeToken(ConfiguredCommandInspector.GetPrimaryOptionName(option));
        return primaryName is "DEVICE" or "DRY_RUN" or "JSON" or "NO_COLOR" or "OUTPUT" or "PROFILE" or "QUIET" or "TIMEOUT" or "VERBOSE" or "YES";
    }

    public static int GetOptionPriority(DocumentationExampleOption option)
    {
        var name = NormalizeToken(option.Attribute.ValueName) ?? NormalizeToken(ConfiguredCommandInspector.GetPrimaryOptionName(option.Attribute));
        if (string.IsNullOrWhiteSpace(name))
            return 50;

        return name switch
        {
            "EMAIL" => 0,
            "ENDPOINT" => 0,
            "URL" => 1,
            "HOSTER" or "HOSTMASK" => 2,
            "USERNAME" or "USER" => 3,
            "PASSWORD" => 4,
            "TYPE" => 5,
            "NAME" or "PROFILE" => 6,
            "ID" or "LINK_ID" or "PACKAGE_ID" or "SUBSCRIPTION_ID" => 7,
            "PATH" or "FILE" => 8,
            "TEXT" or "RESULT" or "CONTENT" => 9,
            "JSON" or "BODY" or "DATA" => 10,
            _ => 20,
        };
    }

    public static string CreateSampleValue(Type type, string? tokenName, string propertyName)
    {
        var normalizedToken = NormalizeToken(tokenName) ?? NormalizeToken(propertyName) ?? "VALUE";
        var elementType = Nullable.GetUnderlyingType(type) ?? type;
        if (elementType.IsArray)
            elementType = elementType.GetElementType() ?? typeof(string);

        if (elementType == typeof(bool))
            return "true";
        if (elementType == typeof(Guid))
            return "00000000-0000-0000-0000-000000000001";
        if (elementType == typeof(int) || elementType == typeof(long) || elementType == typeof(short))
            return "1";
        if (elementType == typeof(decimal) || elementType == typeof(double) || elementType == typeof(float))
            return "1.0";
        if (elementType == typeof(DateTime) || elementType == typeof(DateTimeOffset))
            return "2025-01-01T00:00:00Z";
        if (elementType.IsEnum)
            return Enum.GetNames(elementType).First();

        return normalizedToken switch
        {
            "ALIAS" or "DEVICE" or "DEVICE_NAME" or "NAME" or "PROFILE" or "USERNAME" => "demo",
            "EMAIL" => "demo@example.com",
            "ENDPOINT" => "/downloadsV2/queryLinks",
            "FILE" or "CONTENT_FILE" => "./sample.dlc",
            "FORMAT" => "text",
            "HOSTER" => "ddownload.com",
            "HOSTMASK" => "example.com",
            "ID" or "LINK_ID" or "PACKAGE_ID" or "SUBSCRIPTION_ID" => "12345",
            "INTERFACE_NAME" => "GeneralSettings",
            "JSON" or "BODY" or "DATA" or "QUERY_JSON" or "VALUE_JSON" => "{}",
            "KEY" => "downloadFolder",
            "MASK" => "example.com",
            "MODE" => "json",
            "NAME_OR_ID" => "demo",
            "PASSWORD" => "secret",
            "PATH" => "./sample",
            "RESULT" => "secret",
            "TEXT" => "example",
            "TYPE" => "http",
            "URL" => "https://example.invalid/file",
            "VALUE" => "example",
            _ when normalizedToken.Contains("EMAIL", StringComparison.Ordinal) => "demo@example.com",
            _ when normalizedToken.Contains("PASSWORD", StringComparison.Ordinal) => "secret",
            _ when normalizedToken.Contains("JSON", StringComparison.Ordinal) => "{}",
            _ when normalizedToken.Contains("PATH", StringComparison.Ordinal) => "./sample",
            _ when normalizedToken.Contains("FILE", StringComparison.Ordinal) => "./sample",
            _ when normalizedToken.Contains("URL", StringComparison.Ordinal) => "https://example.invalid/file",
            _ when normalizedToken.Contains("ID", StringComparison.Ordinal) => "12345",
            _ when normalizedToken.Contains("NAME", StringComparison.Ordinal) => "demo",
            _ => normalizedToken.ToLowerInvariant(),
        };
    }

    private static string? NormalizeToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        return token.Trim('<', '>', '[', ']', '-', ' ')
            .Replace("|", string.Empty, StringComparison.Ordinal)
            .Replace(' ', '_')
            .ToUpperInvariant();
    }
}
