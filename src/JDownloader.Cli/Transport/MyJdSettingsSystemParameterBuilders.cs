using JDownloader.Cli.Runtime;

namespace JDownloader.Cli.Transport;

internal static class MyJdSettingsSystemParameterBuilders
{
    public static bool TryBuild(MyJdRequestPlan plan, out (object? Parameters, IReadOnlyList<string>? Warnings) result)
    {
        switch (plan.Endpoint)
        {
            case "/extensions/install":
                result = MyJdParameterSupport.BuildSingleStringParameter(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "id", "settings extensions install requires --id <id>.", out _);
                return true;
            case "/extensions/setEnabled":
                result = BuildExtensionsSetEnabledParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/plugins/get":
                result = BuildPluginsGetParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/system/getStorageInfos":
                result = BuildSystemStorageParameters(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), out _);
                return true;
            case "/system/getSystemInfos":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system info does not accept query/body parameters.");
                return true;
            case "/system/shutdownOS":
                result = MyJdParameterSupport.BuildSingleBooleanParameter(MyJdParameterSupport.EnsureNoBodyForMappedEndpoint(plan), "force", "system os shutdown requires --force (or omit it to send force=false).", out _);
                return true;
            case "/system/hibernateOS":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system os hibernate does not accept query/body parameters.");
                return true;
            case "/system/standbyOS":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system os standby does not accept query/body parameters.");
                return true;
            case "/reconnect/doReconnect":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system reconnect does not accept query/body parameters.");
                return true;
            case "/system/exitJD":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system jd exit does not accept query/body parameters.");
                return true;
            case "/system/restartJD":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system jd restart does not accept query/body parameters.");
                return true;
            case "/toolbar/toggleAutomaticReconnect":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system toggle automatic-reconnect does not accept query/body parameters.");
                return true;
            case "/toolbar/toggleClipboardMonitoring":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system toggle clipboard-monitoring does not accept query/body parameters.");
                return true;
            case "/toolbar/toggleDownloadSpeedLimit":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system toggle speed-limit does not accept query/body parameters.");
                return true;
            case "/toolbar/togglePauseDownloads":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system toggle pause does not accept query/body parameters.");
                return true;
            case "/toolbar/togglePremium":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system toggle premium does not accept query/body parameters.");
                return true;
            case "/toolbar/toggleStopAfterCurrentDownload":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system toggle stop-after-current does not accept query/body parameters.");
                return true;
            case "/jd/version":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system jd version does not accept query/body parameters.");
                return true;
            case "/jd/getCoreRevision":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system jd revision does not accept query/body parameters.");
                return true;
            case "/jd/uptime":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system jd uptime does not accept query/body parameters.");
                return true;
            case "/jd/refreshPlugins":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system jd refresh-plugins does not accept query/body parameters.");
                return true;
            case "/update/isUpdateAvailable":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system update check does not accept query/body parameters.");
                return true;
            case "/update/runUpdateCheck":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system update run does not accept query/body parameters.");
                return true;
            case "/update/restartAndUpdate":
                result = MyJdParameterSupport.EnsureNoParameters(plan, "system update restart does not accept query/body parameters.");
                return true;
            default:
                result = default;
                return false;
        }
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildPluginsGetParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("interfaceName", out var rawInterfaceName)
            && values.TryGetValue("displayName", out var rawDisplayName)
            && values.TryGetValue("key", out var rawKey)
            && rawInterfaceName is not null
            && rawDisplayName is not null
            && rawKey is not null
            && !string.IsNullOrWhiteSpace(rawInterfaceName.ToString())
            && !string.IsNullOrWhiteSpace(rawDisplayName.ToString())
            && !string.IsNullOrWhiteSpace(rawKey.ToString()))
        {
            return (new object?[] { rawInterfaceName.ToString(), rawDisplayName.ToString(), rawKey.ToString() }, null);
        }

        throw CliException.Usage("settings plugins get requires --interface-name <name> --display-name <name> --key <key>.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildSystemStorageParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("path", out var rawPath)
            && rawPath is not null
            && !string.IsNullOrWhiteSpace(rawPath.ToString()))
        {
            return (new object?[] { rawPath.ToString() }, null);
        }

        throw CliException.Usage("system storage requires --path <path>.");
    }

    private static (object? Parameters, IReadOnlyList<string>? Warnings) BuildExtensionsSetEnabledParameters(object? query, out IReadOnlyList<string>? warnings)
    {
        warnings = null;
        if (query is Dictionary<string, object?> values
            && values.TryGetValue("classname", out var rawClassname)
            && rawClassname is not null
            && !string.IsNullOrWhiteSpace(rawClassname.ToString())
            && values.TryGetValue("b", out var rawEnabled)
            && rawEnabled is not null
            && MyJdValueReaders.TryReadBool(rawEnabled, out var enabled))
        {
            return (new object?[] { rawClassname.ToString(), enabled }, null);
        }

        throw CliException.Usage("settings extensions enable/disable requires --classname <name> (or --id <id>) and a boolean state.");
    }
}
