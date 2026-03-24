namespace JDownloader.Cli.Transport;

internal static class MyJdParameterMapper
{
    public static (object? Parameters, IReadOnlyList<string>? Warnings) Build(MyJdRequestPlan plan)
    {
        if (MyJdQueryParameterBuilders.TryBuild(plan, out var result))
            return result;
        if (MyJdAccountParameterBuilders.TryBuild(plan, out result))
            return result;
        if (MyJdCoreActionParameterBuilders.TryBuild(plan, out result))
            return result;
        if (MyJdCaptchaEventsParameterBuilders.TryBuild(plan, out result))
            return result;
        if (MyJdSettingsSystemParameterBuilders.TryBuild(plan, out result))
            return result;
        if (MyJdAdvancedParameterBuilders.TryBuild(plan, out result))
            return result;

        return MyJdParameterSupport.BuildGenericParameters(plan);
    }
}
