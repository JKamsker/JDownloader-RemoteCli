using System.Text;
using JDownloader.Cli.Bootstrap;
using Spectre.Console;

Console.OutputEncoding = Encoding.UTF8;

// Prevent wrapped JSON/XML export when the CLI is executed non-interactively.
if ((Console.IsOutputRedirected || Console.IsErrorRedirected) && AnsiConsole.Profile.Width < 512)
{
    AnsiConsole.Profile.Width = 512;
}

if (args.Any(arg => string.Equals(arg, "--no-color", StringComparison.OrdinalIgnoreCase)))
{
    Environment.SetEnvironmentVariable("NO_COLOR", "1");
    Environment.SetEnvironmentVariable("SPECTRE_CONSOLE_ANSI", "0");
}

return await CliApplication.Create().RunAsync(args);
