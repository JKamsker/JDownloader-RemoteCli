using JDownloader.Cli.Commands.Captcha;
using JDownloader.Cli.Commands.Events;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class CliApplicationCaptchaEventsRegistration
{
    public static void Register(IConfigurator config)
    {
        RegisterCaptcha(config);
        RegisterEvents(config);
    }

    private static void RegisterCaptcha(IConfigurator config)
    {
        config.AddBranch("captcha", captcha =>
        {
            captcha.SetDescription("Inspect and answer captcha jobs.");
            captcha.AddCommand<CaptchaListCommand>("list").WithDescription("List captcha jobs.");
            captcha.AddCommand<CaptchaGetCommand>("get").WithDescription("Get a captcha job.");
            captcha.AddCommand<CaptchaJobCommand>("job").WithDescription("Get captcha job details.");
            captcha.AddCommand<CaptchaSolveCommand>("solve").WithDescription("Submit a captcha answer.");
            captcha.AddCommand<CaptchaSkipCommand>("skip").WithDescription("Skip a captcha.");
            captcha.AddBranch("forward", forward =>
            {
                forward.SetDescription("Captcha forward operations.");
                forward.AddCommand<CaptchaForwardCreateJobCommand>("create-job").WithDescription("Create a captcha forward job (RecaptchaV2).");
                forward.AddCommand<CaptchaForwardGetResultCommand>("get-result").WithDescription("Fetch a captcha forward result by job id.");
            });
        });
    }

    private static void RegisterEvents(IConfigurator config)
    {
        config.AddBranch("events", events =>
        {
            events.SetDescription("Inspect and manage event subscriptions.");
            events.AddCommand<EventsPublishersCommand>("publishers").WithDescription("List event publishers.");
            events.AddCommand<EventsSubscribeCommand>("subscribe").WithDescription("Create a new subscription.");
            events.AddCommand<EventsSetCommand>("set").WithDescription("Set subscription content.");
            events.AddCommand<EventsRemoveCommand>("remove").WithDescription("Remove subscription content.");
            events.AddCommand<EventsStatusCommand>("status").WithDescription("Get subscription status.");
            events.AddCommand<EventsListenCommand>("listen").WithDescription("Listen for events on a subscription id.");
        });
    }
}
