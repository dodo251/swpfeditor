using SwpfEditor.Domain.Enums;

namespace SwpfEditor.Domain.Models;

/// <summary>
/// Validation result item
/// </summary>
public class ValidationResult
{
    public ValidationSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ElementPath { get; set; }
    public string? RuleName { get; set; }
    public string? SuggestedFix { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Collection of validation results
/// </summary>
public class ValidationResults
{
    public List<ValidationResult> Items { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public bool HasErrors => Items.Any(i => i.Severity == ValidationSeverity.Error);
    public bool HasWarnings => Items.Any(i => i.Severity == ValidationSeverity.Warning);
    public bool IsValid => !HasErrors;
    
    public void AddError(string message, string? elementPath = null, string? ruleName = null, string? suggestedFix = null)
    {
        Items.Add(new ValidationResult
        {
            Severity = ValidationSeverity.Error,
            Message = message,
            ElementPath = elementPath,
            RuleName = ruleName,
            SuggestedFix = suggestedFix
        });
    }
    
    public void AddWarning(string message, string? elementPath = null, string? ruleName = null, string? suggestedFix = null)
    {
        Items.Add(new ValidationResult
        {
            Severity = ValidationSeverity.Warning,
            Message = message,
            ElementPath = elementPath,
            RuleName = ruleName,
            SuggestedFix = suggestedFix
        });
    }
    
    public void AddInfo(string message, string? elementPath = null, string? ruleName = null)
    {
        Items.Add(new ValidationResult
        {
            Severity = ValidationSeverity.Info,
            Message = message,
            ElementPath = elementPath,
            RuleName = ruleName
        });
    }
}