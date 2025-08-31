namespace SwpfEditor.Domain.Services;

/// <summary>
/// Placeholder resolution with priority hierarchy
/// </summary>
public class PlaceholderResolver
{
    private readonly Dictionary<string, string> _runtimeLocals = new();
    private readonly Dictionary<string, string> _extractedValues = new();
    private readonly Dictionary<string, string> _inputs = new();
    private readonly Dictionary<string, string> _variables = new();
    private readonly Dictionary<string, string> _constants = new();
    private readonly Dictionary<string, string> _testConfiguration = new();
    private readonly Dictionary<string, string> _environmentVariables = new();

    /// <summary>
    /// Priority order (highest to lowest):
    /// 1. Runtime locals and extracted values
    /// 2. Inputs
    /// 3. Variables  
    /// 4. Constants
    /// 5. TestConfiguration
    /// 6. Environment variables
    /// </summary>
    public string? ResolvePlaceholder(string placeholderName)
    {
        // 1. Runtime locals and extracted values (highest priority)
        if (_runtimeLocals.TryGetValue(placeholderName, out var runtimeValue))
            return runtimeValue;
            
        if (_extractedValues.TryGetValue(placeholderName, out var extractedValue))
            return extractedValue;

        // 2. Inputs
        if (_inputs.TryGetValue(placeholderName, out var inputValue))
            return inputValue;

        // 3. Variables
        if (_variables.TryGetValue(placeholderName, out var variableValue))
            return variableValue;

        // 4. Constants
        if (_constants.TryGetValue(placeholderName, out var constantValue))
            return constantValue;

        // 5. TestConfiguration
        if (_testConfiguration.TryGetValue(placeholderName, out var configValue))
            return configValue;

        // 6. Environment variables (lowest priority)
        if (_environmentVariables.TryGetValue(placeholderName, out var envValue))
            return envValue;

        return null;
    }

    /// <summary>
    /// Resolve all placeholders in a text string
    /// </summary>
    public string ResolveText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var result = text;
        var placeholderPattern = @"\$\{([^}]+)\}";
        var matches = System.Text.RegularExpressions.Regex.Matches(text, placeholderPattern);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var placeholderName = match.Groups[1].Value;
            var resolvedValue = ResolvePlaceholder(placeholderName);
            
            if (resolvedValue != null)
            {
                result = result.Replace(match.Value, resolvedValue);
            }
        }

        return result;
    }

    /// <summary>
    /// Check if all placeholders in text can be resolved
    /// </summary>
    public List<string> GetUnresolvedPlaceholders(string text)
    {
        var unresolved = new List<string>();
        
        if (string.IsNullOrEmpty(text))
            return unresolved;

        var placeholderPattern = @"\$\{([^}]+)\}";
        var matches = System.Text.RegularExpressions.Regex.Matches(text, placeholderPattern);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var placeholderName = match.Groups[1].Value;
            var resolvedValue = ResolvePlaceholder(placeholderName);
            
            if (resolvedValue == null)
            {
                unresolved.Add(placeholderName);
            }
        }

        return unresolved.Distinct().ToList();
    }

    public void SetRuntimeLocal(string name, string value) => _runtimeLocals[name] = value;
    public void SetExtractedValue(string name, string value) => _extractedValues[name] = value;
    public void SetInput(string name, string value) => _inputs[name] = value;
    public void SetVariable(string name, string value) => _variables[name] = value;
    public void SetConstant(string name, string value) => _constants[name] = value;
    public void SetTestConfigurationValue(string name, string value) => _testConfiguration[name] = value;
    public void SetEnvironmentVariable(string name, string value) => _environmentVariables[name] = value;

    public void ClearRuntimeLocals() => _runtimeLocals.Clear();
    public void ClearExtractedValues() => _extractedValues.Clear();
}