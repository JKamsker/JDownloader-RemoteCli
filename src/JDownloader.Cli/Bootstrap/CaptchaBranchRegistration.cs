using JDownloader.Cli.Commands.Captcha;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Bootstrap;

internal static class CliApplicationCaptchaRegistration
{
    public static void Register(IConfigurator config)
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
                forward.AddCommand<CaptchaForwardCreateJobCommand>("create-job").WithDescription("Create a provider-specific RecaptchaV2 captcha forward job.");
                forward.AddCommand<CaptchaForwardGetResultCommand>("get-result").WithDescription("Fetch a captcha forward result by job id.");
            });
        });
    }
}
