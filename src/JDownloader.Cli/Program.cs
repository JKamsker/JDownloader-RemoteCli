using JDownloader.Cli.Bootstrap;

if (args.Any(arg => string.Equals(arg, "--no-color", StringComparison.OrdinalIgnoreCase)))
    Environment.SetEnvironmentVariable("NO_COLOR", "1");

return await CliApplication.Create().RunAsync(args);
