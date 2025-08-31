using System.Text.RegularExpressions;
using SwpfEditor.Domain.Services;

namespace SwpfEditor.Infrastructure.Services;

public class PlaceholderResolver : IPlaceholderResolver
{
    private static readonly Regex PlaceholderRegex = new(@"\$\{([^}]+)\}", RegexOptions.Compiled);

    public string ResolvePlaceholders(string text, PlaceholderContext context)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return PlaceholderRegex.Replace(text, match =>
        {
            var placeholderName = match.Groups[1].Value;
            var resolvedValue = ResolvePlaceholder(placeholderName, context);
            return resolvedValue ?? match.Value; // Keep original if not resolved
        });
    }

    public List<ValidationError> ValidatePlaceholders(string text, PlaceholderContext context)
    {
        var errors = new List<ValidationError>();
        
        if (string.IsNullOrEmpty(text))
            return errors;

        var matches = PlaceholderRegex.Matches(text);
        foreach (Match match in matches)
        {
            var placeholderName = match.Groups[1].Value;
            var resolvedValue = ResolvePlaceholder(placeholderName, context);
            
            if (resolvedValue == null)
            {
                errors.Add(new ValidationError
                {
                    Message = $"Unresolvable placeholder: ${{{placeholderName}}}",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        return errors;
    }

    public HashSet<string> ExtractPlaceholderNames(string text)
    {
        var placeholders = new HashSet<string>();
        
        if (string.IsNullOrEmpty(text))
            return placeholders;

        var matches = PlaceholderRegex.Matches(text);
        foreach (Match match in matches)
        {
            placeholders.Add(match.Groups[1].Value);
        }

        return placeholders;
    }

    private string? ResolvePlaceholder(string name, PlaceholderContext context)
    {
        // Priority order: Runtime > Inputs > Variables > Constants > TestConfig > Environment
        
        // 1. Runtime values (highest priority)
        if (context.RuntimeValues.TryGetValue(name, out var runtimeValue))
            return runtimeValue;

        // 2. Inputs
        if (context.Inputs.TryGetValue(name, out var inputValue))
            return inputValue;

        // 3. Variables
        if (context.Variables.TryGetValue(name, out var variableValue))
            return variableValue;

        // 4. Constants
        if (context.Constants.TryGetValue(name, out var constantValue))
            return constantValue;

        // 5. Test Configuration properties
        if (context.TestConfigProperties.TryGetValue(name, out var configValue))
            return configValue;

        // 6. Environment variables (lowest priority)
        if (context.EnvironmentVariables.TryGetValue(name, out var envValue))
            return envValue;

        return null; // Unresolvable
    }
}