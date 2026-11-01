using System.Diagnostics;
using JDownloader.Cli.Commands.Shared;
using JDownloader.Cli.Runtime;
using JDownloader.Cli.Transport;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Commands.Connection;

/// <summary>Probes the resolved connection step by step and reports latency and the first failure.</summary>
public sealed class ConnectionTestCommand : DeviceApiCommand<DeviceNoArgSettings>
{
    private static readonly (string Name, string Endpoint)[] Probes =
    [
        ("ping", "/device/ping"),
        ("version", "/jd/version"),
        ("core-revision", "/jd/getCoreRevision"),
        ("download-state", "/downloadcontroller/getCurrentState"),
    ];

    private readonly IMyJdTransport _transport;

    public ConnectionTestCommand(
        IProfileResolver profileResolver,
        IOutputRenderer outputRenderer,
        IDiagnosticLogger diagnosticLogger,
        IMyJdTransport transport)
        : base(profileResolver, outputRenderer, diagnosticLogger)
    {
        _transport = transport;
    }

    protected override async Task<CommandOutput> ExecuteCoreAsync(
        CommandContext context,
        DeviceNoArgSettings settings,
        ResolvedProfileContext resolved,
        CancellationToken cancellationToken)
    {
        var target = resolved.IsDirect ? resolved.Direct!.DisplayValue : $"My.JDownloader device {resolved.Device?.DisplayValue}";
        if (settings.DryRun)
        {
            return new CommandOutput(
                new { action = "dry-run", profile = resolved.ProfileName, mode = ConnectionModes.ToName(resolved.ConnectionMode), target, probes = Probes.Select(p => p.Endpoint) },
                ["Dry-run only. No requests were sent.", $"Would probe {target}: {string.Join(", ", Probes.Select(p => p.Endpoint))}"]);
        }

        var steps = new List<object>();
        var lines = new List<string> { $"Profile: {resolved.ProfileName} ({ConnectionModes.ToName(resolved.ConnectionMode)})", $"Target: {target}" };
        var warnings = new List<string>();
        foreach (var (name, endpoint) in Probes)
        {
            var plan = new MyJdRequestPlan($"connection.test.{name}", "POST", endpoint, null, null, false, false, resolved.Device?.Id);
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _transport.ExecuteAsync(resolved, plan, cancellationToken);
                stopwatch.Stop();
                if (result.Warnings is not null)
                    warnings.AddRange(result.Warnings);

                steps.Add(new { step = name, endpoint, ok = true, elapsedMs = stopwatch.ElapsedMilliseconds, value = result.Data });
                lines.Add($"[ok]   {name,-14} {stopwatch.ElapsedMilliseconds,5} ms  {HumanDataRenderer.Render(result.Data)?.FirstOrDefault() ?? string.Empty}".TrimEnd());
            }
            catch (CliException ex) when (ex.Kind is "transport" or "not_authenticated" or "usage")
            {
                lines.Add($"[fail] {name,-14} {ex.Message}");
                throw new CliException(
                    ex.ExitCode,
                    ex.Kind,
                    $"Connection test failed at '{name}' ({endpoint}) after {steps.Count} successful step(s): {ex.Message}"
                    + $"{Environment.NewLine}{string.Join(Environment.NewLine, lines)}",
                    ex.Recovery);
            }
        }

        lines.Add("Connection OK.");
        return new CommandOutput(
            new { profile = resolved.ProfileName, mode = ConnectionModes.ToName(resolved.ConnectionMode), target, ok = true, steps },
            lines,
            warnings.Count == 0 ? null : warnings.Distinct().ToList());
    }
}
