using System.Reflection;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Documentation;

internal static class ConfiguratorDocumentationExtensions
{
    public static void AddDocumentationExamples(this IConfigurator configurator)
    {
        ArgumentNullException.ThrowIfNull(configurator);

        foreach (var command in ConfiguredCommandInspector.GetChildren(configurator, "Commands"))
            PopulateExamples(command, []);
    }

    private static void PopulateExamples(object configuredCommand, IReadOnlyList<string> parentPath)
    {
        if (ConfiguredCommandInspector.IsHidden(configuredCommand))
            return;

        var commandName = ConfiguredCommandInspector.GetName(configuredCommand);
        if (ConfiguredCommandInspector.IsDefaultCommandName(commandName))
            return;

        var path = parentPath.Concat([commandName]).ToArray();
        var children = ConfiguredCommandInspector.GetChildren(configuredCommand, "Children")
            .Where(child => !ConfiguredCommandInspector.IsHidden(child))
            .Where(child => !ConfiguredCommandInspector.IsDefaultCommandName(ConfiguredCommandInspector.GetName(child)))
            .ToList();

        foreach (var child in children)
            PopulateExamples(child, path);

        var examples = ConfiguredCommandInspector.GetExamples(configuredCommand);
        if (examples.Count > 0)
            return;

        var generatedExample = children.Count > 0
            ? BuildBranchExample(children)
            : BuildLeafExample(configuredCommand, path);

        if (generatedExample.Length > 0)
            examples.Add(generatedExample);
    }

    private static string[] BuildBranchExample(IEnumerable<object> children)
    {
        foreach (var child in children)
        {
            var examples = ConfiguredCommandInspector.GetExamples(child);
            if (examples.Count == 0 || examples[0] is not string[] example)
                continue;

            return example.ToArray();
        }

        return [];
    }

    private static string[] BuildLeafExample(object configuredCommand, IReadOnlyList<string> path)
    {
        var pathKey = string.Join(' ', path);
        if (DocumentationExampleConventions.TryGetManualExample(pathKey, out var manualExample))
            return manualExample;

        var settingsType = ConfiguredCommandInspector.GetSettingsType(configuredCommand);
        var arguments = GetArguments(settingsType);
        var options = GetOptions(settingsType);
        var tokens = path.ToList();

        foreach (var argument in arguments)
        {
            tokens.Add(DocumentationExampleConventions.CreateSampleValue(
                argument.Property.PropertyType,
                argument.Attribute.ValueName,
                argument.Property.Name));
        }

        AppendOptions(tokens, options.Where(option => option.Attribute.IsRequired));

        if (DocumentationExampleConventions.ShouldAddIllustrativeOption(path[^1], arguments.Length, options))
        {
            var candidate = GetIllustrativeOption(options);
            if (candidate is not null)
                AppendOptions(tokens, [candidate]);
        }

        return tokens.ToArray();
    }

    private static DocumentationExampleArgument[] GetArguments(Type settingsType)
    {
        return ConfiguredCommandInspector.GetCommandProperties(settingsType)
            .Select(TryCreateArgument)
            .Where(argument => argument is not null)
            .Cast<DocumentationExampleArgument>()
            .OrderBy(argument => argument.Attribute.Position)
            .ToArray();
    }

    private static DocumentationExampleOption[] GetOptions(Type settingsType)
    {
        return ConfiguredCommandInspector.GetCommandProperties(settingsType)
            .Select(property => TryCreateOption(settingsType, property))
            .Where(option => option is not null)
            .Cast<DocumentationExampleOption>()
            .ToArray();
    }

    private static DocumentationExampleArgument? TryCreateArgument(PropertyInfo property)
    {
        var attribute = property.GetCustomAttribute<CommandArgumentAttribute>();
        return attribute is null ? null : new DocumentationExampleArgument(property, attribute);
    }

    private static DocumentationExampleOption? TryCreateOption(Type settingsType, PropertyInfo property)
    {
        var attribute = property.GetCustomAttribute<CommandOptionAttribute>();
        if (attribute is null || attribute.IsHidden)
            return null;

        return new DocumentationExampleOption(property, attribute, property.DeclaringType == settingsType);
    }

    private static DocumentationExampleOption? GetIllustrativeOption(IEnumerable<DocumentationExampleOption> options)
    {
        var candidates = options
            .Where(option => !option.Attribute.IsRequired)
            .Where(option => !DocumentationExampleConventions.IsNoiseOption(option.Attribute))
            .ToArray();

        if (candidates.Length == 0)
            return null;

        var commandSpecificCandidates = candidates.Where(option => option.DeclaredOnCommand).ToArray();
        return (commandSpecificCandidates.Length > 0 ? commandSpecificCandidates : candidates)
            .OrderBy(DocumentationExampleConventions.GetOptionPriority)
            .ThenBy(option => option.Property.MetadataToken)
            .FirstOrDefault();
    }

    private static void AppendOptions(ICollection<string> tokens, IEnumerable<DocumentationExampleOption> options)
    {
        foreach (var option in options)
        {
            tokens.Add(ConfiguredCommandInspector.GetPrimaryOptionName(option.Attribute));
            if (IsFlagOption(option.Property.PropertyType, option.Attribute))
                continue;

            tokens.Add(DocumentationExampleConventions.CreateSampleValue(
                option.Property.PropertyType,
                option.Attribute.ValueName,
                option.Property.Name));
        }
    }

    private static bool IsFlagOption(Type propertyType, CommandOptionAttribute attribute)
    {
        return !attribute.ValueIsOptional && (propertyType == typeof(bool) || propertyType == typeof(bool?));
    }
}
