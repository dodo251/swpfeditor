using SwpfEditor.Domain.Enums;
using SwpfEditor.Domain.Models;

namespace SwpfEditor.Domain.Services;

/// <summary>
/// Manages coordination between test.xml and TestConfiguration.xml
/// </summary>
public class DualXmlCoordinator
{
    private readonly PlaceholderResolver _placeholderResolver;

    public DualXmlCoordinator(PlaceholderResolver placeholderResolver)
    {
        _placeholderResolver = placeholderResolver;
    }

    /// <summary>
    /// Auto-generate sessions from configuration if test sessions are empty
    /// </summary>
    public List<Session> GenerateSessionsFromConfiguration(Test test, TestConfiguration config)
    {
        var generatedSessions = new List<Session>();

        // If test already has sessions, return them
        if (test.Sessions.Any())
            return test.Sessions;

        // Generate sessions from configuration connections
        foreach (var sshConn in config.SshList)
        {
            generatedSessions.Add(new Session
            {
                Name = sshConn.Name,
                Type = TargetType.Ssh,
                Host = sshConn.Host,
                Port = sshConn.Port,
                User = sshConn.Username,
                Password = sshConn.Password,
                Prompt = sshConn.Prompt
            });
        }

        foreach (var httpConn in config.HttpList)
        {
            generatedSessions.Add(new Session
            {
                Name = httpConn.Name,
                Type = TargetType.Http,
                BaseUrl = httpConn.BaseUrl,
                User = httpConn.Username,
                Password = httpConn.Password
            });
        }

        foreach (var telnetConn in config.TelnetList)
        {
            generatedSessions.Add(new Session
            {
                Name = telnetConn.Name,
                Type = TargetType.Telnet,
                Host = telnetConn.Host,
                Port = telnetConn.Port,
                User = telnetConn.Username,
                Password = telnetConn.Password,
                Prompt = telnetConn.Prompt
            });
        }

        return generatedSessions;
    }

    /// <summary>
    /// Validate step target mappings against configuration
    /// </summary>
    public List<string> ValidateStepTargetMappings(Test test, TestConfiguration config)
    {
        var errors = new List<string>();
        var allSessions = GenerateSessionsFromConfiguration(test, config);

        foreach (var step in test.Steps)
        {
            // Find matching session by target type and name
            var matchingSession = allSessions.FirstOrDefault(s => 
                s.Name == step.Target && 
                s.Type == step.TargetType);

            if (matchingSession == null)
            {
                // Try to find by name only
                var sessionByName = allSessions.FirstOrDefault(s => s.Name == step.Target);
                if (sessionByName == null)
                {
                    errors.Add($"Step '{step.Id}' references unknown target '{step.Target}'");
                }
                else
                {
                    errors.Add($"Step '{step.Id}' target '{step.Target}' type mismatch: expected {step.TargetType}, found {sessionByName.Type}");
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Initialize placeholder resolver from configuration
    /// </summary>
    public void InitializePlaceholderResolver(TestConfiguration config)
    {
        // Load inputs
        foreach (var input in config.Inputs)
        {
            _placeholderResolver.SetInput(input.Name, input.Value);
        }

        // Load variables
        foreach (var variable in config.Variables)
        {
            _placeholderResolver.SetVariable(variable.Name, variable.Value);
        }

        // Load constants
        foreach (var constant in config.Constants)
        {
            _placeholderResolver.SetConstant(constant.Name, constant.Value);
        }

        // Load configuration values
        if (config.LogDirectory != null)
        {
            _placeholderResolver.SetTestConfigurationValue("LogDirectory", config.LogDirectory);
        }

        // Load UUT properties
        if (config.UUT?.Properties != null)
        {
            foreach (var prop in config.UUT.Properties)
            {
                _placeholderResolver.SetTestConfigurationValue($"UUT.{prop.Key}", prop.Value?.ToString() ?? "");
            }
        }

        // Load environment variables
        foreach (System.Collections.DictionaryEntry envVar in Environment.GetEnvironmentVariables())
        {
            _placeholderResolver.SetEnvironmentVariable(envVar.Key?.ToString() ?? "", envVar.Value?.ToString() ?? "");
        }
    }

    /// <summary>
    /// Validate that all placeholders in test can be resolved
    /// </summary>
    public List<ValidationResult> ValidatePlaceholders(Test test)
    {
        var results = new List<ValidationResult>();

        // Check steps
        foreach (var step in test.Steps)
        {
            ValidateStepPlaceholders(step, results);
        }

        return results;
    }

    private void ValidateStepPlaceholders(Step step, List<ValidationResult> results)
    {
        var stepPath = $"steps/step[@id='{step.Id}']";

        // Check target
        var unresolvedInTarget = _placeholderResolver.GetUnresolvedPlaceholders(step.Target);
        foreach (var placeholder in unresolvedInTarget)
        {
            results.Add(new ValidationResult
            {
                Severity = ValidationSeverity.Error,
                Message = $"Unresolved placeholder '${{{placeholder}}}' in step target",
                ElementPath = $"{stepPath}/@target",
                RuleName = "PlaceholderResolution",
                SuggestedFix = $"Define '{placeholder}' in inputs, variables, constants, or TestConfiguration"
            });
        }

        // Check command
        if (!string.IsNullOrEmpty(step.Command))
        {
            var unresolvedInCommand = _placeholderResolver.GetUnresolvedPlaceholders(step.Command);
            foreach (var placeholder in unresolvedInCommand)
            {
                results.Add(new ValidationResult
                {
                    Severity = ValidationSeverity.Error,
                    Message = $"Unresolved placeholder '${{{placeholder}}}' in step command",
                    ElementPath = $"{stepPath}/@command",
                    RuleName = "PlaceholderResolution",
                    SuggestedFix = $"Define '{placeholder}' in inputs, variables, constants, or TestConfiguration"
                });
            }
        }

        // Check extracts
        foreach (var extract in step.Extracts)
        {
            var extractPath = $"{stepPath}/extracts/extract[@name='{extract.Name}']";
            
            var unresolvedInPattern = _placeholderResolver.GetUnresolvedPlaceholders(extract.Pattern);
            foreach (var placeholder in unresolvedInPattern)
            {
                results.Add(new ValidationResult
                {
                    Severity = ValidationSeverity.Error,
                    Message = $"Unresolved placeholder '${{{placeholder}}}' in extract pattern",
                    ElementPath = $"{extractPath}/@pattern",
                    RuleName = "PlaceholderResolution",
                    SuggestedFix = $"Define '{placeholder}' in inputs, variables, constants, or TestConfiguration"
                });
            }
        }
    }
}