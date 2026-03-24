using JDownloader.Cli.Commands.Accounts;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class AccountsBranchRegistration
{
    public static void RegisterAccountsCommands(this IConfigurator config)
    {
        config.AddBranch("accounts", accounts =>
        {
            accounts.SetDescription("Manage premium accounts and basic-auth entries.");
            accounts.AddCommand<AccountsListCommand>("list").WithDescription("List accounts.");
            accounts.AddCommand<AccountsGetCommand>("get").WithDescription("Deprecated alias for 'accounts hosters url'; kept for backwards compatibility.");
            accounts.AddCommand<AccountsAddCommand>("add").WithDescription("Add an account.");
            accounts.AddCommand<AccountsUpdateCommand>("update").WithDescription("Update username/password for an account id.");
            accounts.AddCommand<AccountsEnableCommand>("enable").WithDescription("Enable accounts by id.");
            accounts.AddCommand<AccountsDisableCommand>("disable").WithDescription("Disable accounts by id.");
            accounts.AddCommand<AccountsRemoveCommand>("remove").WithDescription("Remove accounts by id.");
            accounts.AddCommand<AccountsRefreshCommand>("refresh").WithDescription("Refresh accounts by id.");
            accounts.AddBranch("hosters", hosters =>
            {
                hosters.SetDescription("Inspect premium hosters.");
                hosters.AddCommand<AccountsHostersListCommand>("list").WithDescription("List premium hosters.");
                hosters.AddCommand<AccountsHostersUrlCommand>("url").WithDescription("Resolve a premium hoster name to its account URL.");
                hosters.AddCommand<AccountsHostersUrlsCommand>("urls").WithDescription("List premium hoster URLs.");
            });
            accounts.AddBranch("basic-auth", basicAuth =>
            {
                basicAuth.SetDescription("Manage HTTP/FTP basic-auth entries.");
                basicAuth.AddCommand<AccountsBasicAuthListCommand>("list").WithDescription("List basic-auth entries.");
                basicAuth.AddCommand<AccountsBasicAuthAddCommand>("add").WithDescription("Add a basic-auth entry.");
                basicAuth.AddCommand<AccountsBasicAuthUpdateCommand>("update").WithDescription("Update a basic-auth entry.");
                basicAuth.AddCommand<AccountsBasicAuthRemoveCommand>("remove").WithDescription("Remove basic-auth entries by id.");
            });
        });
    }
}
