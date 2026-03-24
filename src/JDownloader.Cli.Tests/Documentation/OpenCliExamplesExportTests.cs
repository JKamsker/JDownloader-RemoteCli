using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JDownloader.Cli.Tests.Documentation;

public sealed class OpenCliExamplesExportTests
{
    private static readonly Lazy<Task<OpenCliDocument>> ExportDocument = new(LoadExportDocumentAsync);

    [Fact]
    public async Task VisibleCommands_HaveAtLeastOneExample()
    {
        var document = await ExportDocument.Value;

        var missingExamples = EnumerateVisibleCommands(document.Commands, [])
            .Where(command => command.Command.Examples.Count == 0 ||
                              command.Command.Examples.All(string.IsNullOrWhiteSpace))
            .Select(command => command.Path)
            .ToArray();

        Assert.True(
            missingExamples.Length == 0,
            $"Commands without examples:{Environment.NewLine}{string.Join(Environment.NewLine, missingExamples)}");
    }

    [Theory]
    [InlineData("auth", "auth login --email demo@example.com --password-stdin")]
    [InlineData("auth login", "auth login --email demo@example.com --password-stdin")]
    [InlineData("device use", "device use --device jd-main")]
    [InlineData("grabber add", "grabber add --url https://example.invalid/file")]
    [InlineData("accounts add", "accounts add --hoster ddownload.com --username demo --password-stdin")]
    [InlineData("advanced raw request", "advanced raw request /downloadsV2/queryLinks --query-json {}")]
    public async Task KeyCommands_ExposeMeaningfulExamples(string commandPath, string expectedExample)
    {
        var document = await ExportDocument.Value;
        var command = EnumerateVisibleCommands(document.Commands, [])
            .Single(command => string.Equals(command.Path, commandPath, StringComparison.OrdinalIgnoreCase));

        Assert.Contains(expectedExample, command.Command.Examples);
    }

    private static IEnumerable<VisibleCommand> EnumerateVisibleCommands(IEnumerable<OpenCliCommand> commands, IReadOnlyList<string> parentPath)
    {
        foreach (var command in commands)
        {
            if (command.Hidden || IsDefaultCommandName(command.Name))
                continue;

            var path = parentPath.Concat([command.Name]).ToArray();
            yield return new VisibleCommand(string.Join(' ', path), command);

            foreach (var child in EnumerateVisibleCommands(command.Commands, path))
                yield return child;
        }
    }

    private static bool IsDefaultCommandName(string commandName)
    {
        return string.Equals(commandName, "__default_command", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<OpenCliDocument> LoadExportDocumentAsync()
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        startInfo.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "jdr.dll"));
        startInfo.ArgumentList.Add("cli");
        startInfo.ArgumentList.Add("opencli");

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start JDownloader CLI export process.");

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(60));

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        Assert.True(
            process.ExitCode == 0,
            $"`cli opencli` failed with exit code {process.ExitCode}.{Environment.NewLine}{stderr}");

        return JsonSerializer.Deserialize<OpenCliDocument>(
                   stdout,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new InvalidOperationException("Failed to parse cli opencli output.");
    }

    private sealed record VisibleCommand(string Path, OpenCliCommand Command);

    private sealed class OpenCliDocument
    {
        [JsonPropertyName("commands")]
        public List<OpenCliCommand> Commands { get; init; } = [];
    }

    private sealed class OpenCliCommand
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("hidden")]
        public bool Hidden { get; init; }

        [JsonPropertyName("examples")]
        public List<string> Examples { get; init; } = [];

        [JsonPropertyName("commands")]
        public List<OpenCliCommand> Commands { get; init; } = [];
    }
}
