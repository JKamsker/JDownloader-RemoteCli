using System.Collections;
using System.Reflection;
using Spectre.Console.Cli;

namespace JDownloader.Cli.Documentation;

internal sealed record DocumentationExampleArgument(PropertyInfo Property, CommandArgumentAttribute Attribute);

internal sealed record DocumentationExampleOption(PropertyInfo Property, CommandOptionAttribute Attribute, bool DeclaredOnCommand);

internal static class ConfiguredCommandInspector
{
    public static IList GetExamples(object configuredCommand)
    {
        return (IList)(GetRequiredProperty(configuredCommand, "Examples").GetValue(configuredCommand)
            ?? throw new InvalidOperationException("Configured command examples are not available."));
    }

    public static IEnumerable<object> GetChildren(object target, string propertyName)
    {
        return ((IEnumerable)(GetRequiredProperty(target, propertyName).GetValue(target)
                ?? throw new InvalidOperationException($"Property '{propertyName}' is not available.")))
            .Cast<object>();
    }

    public static Type GetSettingsType(object configuredCommand)
    {
        return (Type)(GetRequiredProperty(configuredCommand, "SettingsType").GetValue(configuredCommand)
            ?? throw new InvalidOperationException("Configured command settings type is not available."));
    }

    public static string GetName(object configuredCommand)
    {
        return (string)(GetRequiredProperty(configuredCommand, "Name").GetValue(configuredCommand)
            ?? throw new InvalidOperationException("Configured command name is not available."));
    }

    public static bool IsHidden(object configuredCommand)
    {
        return (bool)(GetRequiredProperty(configuredCommand, "IsHidden").GetValue(configuredCommand)
            ?? false);
    }

    public static bool IsDefaultCommandName(string commandName)
    {
        return string.Equals(commandName, "__default_command", StringComparison.OrdinalIgnoreCase);
    }

    public static IEnumerable<PropertyInfo> GetCommandProperties(Type settingsType)
    {
        var hierarchy = new Stack<Type>();
        for (var type = settingsType; type is not null && type != typeof(object); type = type.BaseType)
            hierarchy.Push(type);

        while (hierarchy.Count > 0)
        {
            foreach (var property in hierarchy.Pop()
                         .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                         .OrderBy(property => property.MetadataToken))
            {
                yield return property;
            }
        }
    }

    public static string GetPrimaryOptionName(CommandOptionAttribute option)
    {
        if (option.LongNames.Count > 0)
            return $"--{option.LongNames[0]}";

        return $"-{option.ShortNames[0]}";
    }

    private static PropertyInfo GetRequiredProperty(object target, string propertyName)
    {
        return target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException($"Property '{propertyName}' was not found on '{target.GetType().FullName}'.");
    }
}
