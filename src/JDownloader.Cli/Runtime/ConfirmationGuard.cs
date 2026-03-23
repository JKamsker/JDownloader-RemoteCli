namespace JDownloader.Cli.Runtime;

public interface IConfirmationGuard
{
    Task<bool> AuthorizeAsync(GlobalSettings settings, string prompt);
}

public sealed class ConfirmationGuard : IConfirmationGuard
{
    private readonly ICliEnvironment _environment;

    public ConfirmationGuard(ICliEnvironment environment)
    {
        _environment = environment;
    }

    public Task<bool> AuthorizeAsync(GlobalSettings settings, string prompt)
    {
        if (settings.DryRun)
            return Task.FromResult(false);

        if (settings.Yes)
            return Task.FromResult(true);

        if (settings.Quiet || _environment.IsInputRedirected || _environment.IsErrorRedirected)
        {
            throw CliException.Usage(
                "Confirmation required in non-interactive mode.",
                "Use --yes to confirm or --dry-run to preview.");
        }

        Console.Error.Write($"{prompt} Type 'yes' to confirm: ");
        Console.Error.Flush();
        var response = Console.ReadLine()?.Trim() ?? string.Empty;
        if (string.Equals(response, "yes", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(true);

        throw CliException.Cancelled("Cancelled.");
    }
}
