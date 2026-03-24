using JDownloader.Cli.Commands.Auth;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class CliApplicationAuthRegistration
{
    public static void Register(IConfigurator config)
    {
        config.AddBranch("auth", auth =>
        {
            auth.SetDescription("Authentication, identity, and saved profiles.");
            auth.AddCommand<LoginCommand>("login").WithDescription("Store encrypted auth material for a profile.");
            auth.AddCommand<LogoutCommand>("logout").WithDescription("Remove stored auth material for the resolved profile.");
            auth.AddCommand<AuthStatusCommand>("status").WithDescription("Show stored auth state for the resolved profile.");
            auth.AddCommand<WhoAmICommand>("whoami").WithDescription("Show the resolved profile and stored account.");
            auth.AddBranch("profiles", profiles =>
            {
                profiles.SetDescription("Manage saved CLI profiles.");
                profiles.AddCommand<ListProfilesCommand>("list").WithDescription("List saved profiles.");
                profiles.AddCommand<GetProfileCommand>("get").WithDescription("Show a saved profile.");
                profiles.AddCommand<AddProfileCommand>("add").WithDescription("Create a new profile.");
                profiles.AddCommand<RenameProfileCommand>("rename").WithDescription("Rename an existing profile.");
                profiles.AddCommand<RemoveProfileCommand>("remove").WithDescription("Remove a profile and its device defaults.");
                profiles.AddCommand<UseProfileCommand>("use").WithDescription("Set the default profile.");
            });
        });
    }
}
