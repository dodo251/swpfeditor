using SwpfEditor.Domain.Models;

namespace SwpfEditor.Domain.Services;

public interface IConnectionResolver
{
    /// <summary>
    /// Resolves connection details for a step based on its target and targetType
    /// </summary>
    /// <param name="step">The step to resolve connections for</param>
    /// <param name="testConfiguration">The test configuration containing connection definitions</param>
    /// <returns>Resolved connection details or null if not found</returns>
    ConnectionDetails? ResolveConnection(Step step, TestConfiguration testConfiguration);
    
    /// <summary>
    /// Auto-generates session mappings when sessions element is empty
    /// </summary>
    /// <param name="test">The test document</param>
    /// <param name="testConfiguration">The test configuration</param>
    /// <returns>Generated sessions</returns>
    Sessions GenerateSessionMappings(Test test, TestConfiguration testConfiguration);
    
    /// <summary>
    /// Validates that all step targets can be resolved
    /// </summary>
    /// <param name="test">The test document</param>
    /// <param name="testConfiguration">The test configuration</param>
    /// <returns>List of validation errors</returns>
    List<ValidationError> ValidateConnections(Test test, TestConfiguration testConfiguration);
}

public class ConnectionDetails
{
    public string Name { get; set; } = string.Empty;
    public TargetType Type { get; set; }
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? BaseUrl { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    public string? Prompt { get; set; }
}

public class ValidationError
{
    public string Message { get; set; } = string.Empty;
    public string? ElementId { get; set; }
    public string? PropertyName { get; set; }
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
    public int? LineNumber { get; set; }
}

public enum ValidationSeverity
{
    Info,
    Warning,
    Error
}