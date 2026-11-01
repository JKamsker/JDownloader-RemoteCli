namespace JDownloader.Cli.Runtime;

public static class DirectModeGuard
{
    /// <summary>Throws an explicit usage error when a My.JDownloader-only command runs on a direct profile.</summary>
    public static void RequireMyJd(ResolvedProfileContext resolved, string commandName)
    {
        if (!resolved.IsDirect)
            return;

        throw CliException.Usage(
            $"'{commandName}' is only available for My.JDownloader connections; profile '{resolved.ProfileName}' uses a direct connection ({resolved.Direct?.DisplayValue}).",
            "Direct connections need no account or device. Use 'jdr connection test' to check connectivity.");
    }
}

public static class DirectProfileContext
{
    /// <summary>Synthetic device id used for direct connections, which have no My.JDownloader device.</summary>
    public const string DirectDeviceId = "direct";

    public static ResolvedProfileContext Create(
        GlobalSettings settings,
        string profileName,
        string profileSource,
        Config.ProfileRecord profile,
        OutputMode outputMode,
        string outputSource,
        int timeoutSeconds,
        string timeoutSource)
    {
        if (!string.IsNullOrWhiteSpace(settings.Device))
        {
            throw CliException.Usage(
                $"--device is not supported for direct profile '{profileName}': a direct connection talks to a single JDownloader instance.",
                "Remove --device, or use a profile with '--mode myjd'.");
        }

        var endpoint = DirectEndpoint.FromProfile(profileName, profile);
        return new ResolvedProfileContext(
            profileName,
            profileSource,
            null,
            outputMode,
            outputSource,
            timeoutSeconds,
            timeoutSource,
            new ResolvedDevice(DirectDeviceId, endpoint.DisplayValue),
            "direct-endpoint",
            ConnectionMode.Direct,
            endpoint);
    }
}
