using SwpfEditor.Domain.Models;
using SwpfEditor.Domain.Enums;

namespace SwpfEditor.Domain.Services;

public interface IPlaceholderResolver
{
    /// <summary>
    /// Resolves placeholder values with the specified priority hierarchy:
    /// Runtime/Local/Extract > Inputs > Variables > Constants > TestConfiguration > Environment
    /// </summary>
    /// <param name="text">Text containing placeholders in ${name} format</param>
    /// <param name="context">Resolution context containing all sources</param>
    /// <returns>Text with resolved placeholders</returns>
    string ResolvePlaceholders(string text, PlaceholderContext context);
    
    /// <summary>
    /// Validates that all placeholders in the text can be resolved
    /// </summary>
    /// <param name="text">Text containing placeholders</param>
    /// <param name="context">Resolution context</param>
    /// <returns>List of validation errors for unresolvable placeholders</returns>
    List<ValidationError> ValidatePlaceholders(string text, PlaceholderContext context);
    
    /// <summary>
    /// Extracts all placeholder names from the text
    /// </summary>
    /// <param name="text">Text to analyze</param>
    /// <returns>Set of placeholder names found</returns>
    HashSet<string> ExtractPlaceholderNames(string text);
}

public class PlaceholderContext
{
    /// <summary>
    /// Runtime values (highest priority) - includes local variables and extract results
    /// </summary>
    public Dictionary<string, string> RuntimeValues { get; set; } = new();
    
    /// <summary>
    /// Test configuration inputs
    /// </summary>
    public Dictionary<string, string> Inputs { get; set; } = new();
    
    /// <summary>
    /// Test configuration variables
    /// </summary>
    public Dictionary<string, string> Variables { get; set; } = new();
    
    /// <summary>
    /// Test configuration constants
    /// </summary>
    public Dictionary<string, string> Constants { get; set; } = new();
    
    /// <summary>
    /// TestConfiguration properties (UUT details, LogDirectory, etc.)
    /// </summary>
    public Dictionary<string, string> TestConfigProperties { get; set; } = new();
    
    /// <summary>
    /// Environment variables (lowest priority)
    /// </summary>
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
    
    /// <summary>
    /// Creates a context from test configuration
    /// </summary>
    public static PlaceholderContext FromTestConfiguration(TestConfiguration config)
    {
        var context = new PlaceholderContext();
        
        // Populate inputs
        if (config.Inputs?.InputList != null)
        {
            foreach (var input in config.Inputs.InputList)
            {
                context.Inputs[input.Name] = input.Value;
            }
        }
        
        // Populate variables
        if (config.Variables?.VariableList != null)
        {
            foreach (var variable in config.Variables.VariableList)
            {
                context.Variables[variable.Name] = variable.Value;
            }
        }
        
        // Populate constants
        if (config.Constants?.ConstantList != null)
        {
            foreach (var constant in config.Constants.ConstantList)
            {
                context.Constants[constant.Name] = constant.Value;
            }
        }
        
        // Populate test config properties
        if (config.LogDirectory != null)
        {
            context.TestConfigProperties["LogDirectory"] = config.LogDirectory;
        }
        
        if (config.UUT != null)
        {
            context.TestConfigProperties["UUT.Name"] = config.UUT.Name;
            context.TestConfigProperties["UUT.Type"] = config.UUT.Type;
            if (config.UUT.Host != null) context.TestConfigProperties["UUT.Host"] = config.UUT.Host;
            if (config.UUT.Port.HasValue) context.TestConfigProperties["UUT.Port"] = config.UUT.Port.ToString()!;
        }
        
        // Populate environment variables
        foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
        {
            if (env.Key is string key && env.Value is string value)
            {
                context.EnvironmentVariables[key] = value;
            }
        }
        
        return context;
    }
}