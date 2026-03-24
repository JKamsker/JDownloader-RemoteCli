namespace JDownloader.Cli.Transport;

internal static class MyJdParameterPreparation
{
    public static (object? Parameters, IReadOnlyList<string>? Warnings) Prepare(MyJdRequestPlan plan)
    {
        return plan.PreserveRawParameters
            ? MyJdParameterSupport.BuildRawParameters(plan)
            : MyJdParameterMapper.Build(plan);
    }
}
