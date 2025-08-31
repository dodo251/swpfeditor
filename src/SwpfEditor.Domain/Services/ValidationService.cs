using SwpfEditor.Domain.Enums;
using SwpfEditor.Domain.Models;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using System.Xml.Linq;

namespace SwpfEditor.Domain.Services;

/// <summary>
/// Validates test XML against XSD schemas and business rules
/// </summary>
public class ValidationService
{
    private readonly DualXmlCoordinator _dualXmlCoordinator;

    public ValidationService(DualXmlCoordinator dualXmlCoordinator)
    {
        _dualXmlCoordinator = dualXmlCoordinator;
    }

    /// <summary>
    /// Validate test document with both XSD and business rules
    /// </summary>
    public ValidationResults ValidateTest(Test test, TestConfiguration? configuration = null)
    {
        var results = new ValidationResults();

        // Validate basic structure and constraints
        ValidateTestStructure(test, results);
        
        // Validate reference integrity
        ValidateReferenceIntegrity(test, results);
        
        // Validate business rules
        ValidateBusinessRules(test, results);
        
        // Validate against configuration if provided
        if (configuration != null)
        {
            ValidateAgainstConfiguration(test, configuration, results);
        }

        return results;
    }

    /// <summary>
    /// Validate XML document against XSD schema
    /// </summary>
    public ValidationResults ValidateAgainstXsd(XDocument document, string schemaPath)
    {
        var results = new ValidationResults();

        try
        {
            var schemas = new XmlSchemaSet();
            schemas.Add(null, schemaPath);

            document.Validate(schemas, (sender, e) =>
            {
                var severity = e.Severity == XmlSeverityType.Error 
                    ? ValidationSeverity.Error 
                    : ValidationSeverity.Warning;
                    
                results.Items.Add(new ValidationResult
                {
                    Severity = severity,
                    Message = e.Message,
                    ElementPath = GetElementPath(e.Exception?.SourceSchemaObject),
                    RuleName = "XSD"
                });
            });
        }
        catch (Exception ex)
        {
            results.AddError($"XSD validation failed: {ex.Message}", ruleName: "XSD");
        }

        return results;
    }

    private void ValidateTestStructure(Test test, ValidationResults results)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(test.Id))
        {
            results.AddError("Test ID is required", "test/@id", "RequiredField");
        }

        // Validate ID format
        if (!string.IsNullOrWhiteSpace(test.Id) && !IsValidIdFormat(test.Id))
        {
            results.AddError($"Test ID '{test.Id}' contains invalid characters", "test/@id", "IdFormat", 
                "Use only alphanumeric characters, underscores, and hyphens");
        }

        // Validate step IDs are unique
        var stepIds = test.Steps.Select(s => s.Id).ToList();
        var duplicateStepIds = stepIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key);
        foreach (var duplicateId in duplicateStepIds)
        {
            results.AddError($"Duplicate step ID: '{duplicateId}'", $"steps/step[@id='{duplicateId}']", "UniqueId");
        }

        // Validate section IDs are unique
        var sectionIds = test.Sections.Select(s => s.Id).ToList();
        var duplicateSectionIds = sectionIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key);
        foreach (var duplicateId in duplicateSectionIds)
        {
            results.AddError($"Duplicate section ID: '{duplicateId}'", $"sections/section[@id='{duplicateId}']", "UniqueId");
        }
    }

    private void ValidateReferenceIntegrity(Test test, ValidationResults results)
    {
        var stepIds = test.Steps.Select(s => s.Id).ToHashSet();
        var stepAliases = test.Steps.Where(s => !string.IsNullOrEmpty(s.Alias)).Select(s => s.Alias!).ToHashSet();
        var sectionIds = test.Sections.Select(s => s.Id).ToHashSet();

        // Validate section references
        foreach (var section in test.Sections)
        {
            var sectionPath = $"sections/section[@id='{section.Id}']";

            // Validate passNext/failNext references
            if (!string.IsNullOrEmpty(section.PassNext) && !sectionIds.Contains(section.PassNext))
            {
                results.AddError($"Section '{section.Id}' passNext references unknown section '{section.PassNext}'", 
                    $"{sectionPath}/@passNext", "ReferenceIntegrity");
            }

            if (!string.IsNullOrEmpty(section.FailNext) && !sectionIds.Contains(section.FailNext))
            {
                results.AddError($"Section '{section.Id}' failNext references unknown section '{section.FailNext}'", 
                    $"{sectionPath}/@failNext", "ReferenceIntegrity");
            }

            // Validate step references in refs
            foreach (var stepRef in section.Refs)
            {
                var refPath = $"{sectionPath}/refs/ref[@step='{stepRef.Step}']";
                
                if (stepRef.Mode == RefMode.Id)
                {
                    if (!stepIds.Contains(stepRef.Step))
                    {
                        results.AddError($"Section '{section.Id}' references unknown step ID '{stepRef.Step}'", 
                            refPath, "ReferenceIntegrity");
                    }
                }
                else if (stepRef.Mode == RefMode.Alias)
                {
                    if (!stepAliases.Contains(stepRef.Step))
                    {
                        results.AddError($"Section '{section.Id}' references unknown step alias '{stepRef.Step}'", 
                            refPath, "ReferenceIntegrity");
                    }
                }
            }
        }

        // Validate extract/check references within steps
        foreach (var step in test.Steps)
        {
            var stepPath = $"steps/step[@id='{step.Id}']";
            var extractNames = step.Extracts.Select(e => e.Name).ToHashSet();

            foreach (var extract in step.Extracts)
            {
                // Validate extract name uniqueness within step
                var sameNameExtracts = step.Extracts.Where(e => e.Name == extract.Name).Count();
                if (sameNameExtracts > 1)
                {
                    results.AddError($"Duplicate extract name '{extract.Name}' in step '{step.Id}'", 
                        $"{stepPath}/extracts/extract[@name='{extract.Name}']", "UniqueExtractName");
                }

                // Validate check sourceRef references
                foreach (var check in extract.Checks)
                {
                    if (!extractNames.Contains(check.SourceRef))
                    {
                        results.AddError($"Check references unknown extract '{check.SourceRef}' in step '{step.Id}'", 
                            $"{stepPath}/extracts/extract[@name='{extract.Name}']/checks/check[@sourceRef='{check.SourceRef}']", 
                            "ReferenceIntegrity");
                    }
                }
            }
        }
    }

    private void ValidateBusinessRules(Test test, ValidationResults results)
    {
        foreach (var step in test.Steps)
        {
            var stepPath = $"steps/step[@id='{step.Id}']";

            // HTTP steps must have method
            if (step.TargetType == TargetType.Http && step.Method == null)
            {
                results.AddError($"HTTP step '{step.Id}' must specify method", $"{stepPath}/@method", "ConditionalRequired");
            }

            // Timeout must be non-negative
            if (step.Timeout < 0)
            {
                results.AddError($"Step '{step.Id}' timeout must be non-negative", $"{stepPath}/@timeout", "ValueRange");
            }

            // Validate extract patterns are valid regex
            foreach (var extract in step.Extracts)
            {
                if (!IsValidRegexPattern(extract.Pattern))
                {
                    results.AddError($"Extract '{extract.Name}' pattern is not a valid regex", 
                        $"{stepPath}/extracts/extract[@name='{extract.Name}']/@pattern", "RegexPattern",
                        "Ensure the pattern is a valid regular expression");
                }

                // Validate expect formats
                foreach (var check in extract.Checks)
                {
                    if (!IsValidExpectFormat(check.Expect))
                    {
                        results.AddError($"Check expect value '{check.Expect}' has invalid format", 
                            $"{stepPath}/extracts/extract[@name='{extract.Name}']/checks/check[@expect='{check.Expect}']", 
                            "ExpectFormat",
                            "Use format: 'REGEX:pattern', 'GE|LE|GT|LT|EQ|NE:number', 'CONTAINS|STARTS|ENDS:text', or literal text");
                    }
                }
            }
        }
    }

    private void ValidateAgainstConfiguration(Test test, TestConfiguration configuration, ValidationResults results)
    {
        // Validate step target mappings
        var targetErrors = _dualXmlCoordinator.ValidateStepTargetMappings(test, configuration);
        foreach (var error in targetErrors)
        {
            results.AddError(error, ruleName: "TargetMapping");
        }

        // Validate placeholders
        var placeholderResults = _dualXmlCoordinator.ValidatePlaceholders(test);
        results.Items.AddRange(placeholderResults);
    }

    private bool IsValidIdFormat(string id)
    {
        return Regex.IsMatch(id, @"^[A-Za-z0-9_\-]+$");
    }

    private bool IsValidRegexPattern(string pattern)
    {
        try
        {
            _ = new Regex(pattern);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidExpectFormat(string expect)
    {
        if (string.IsNullOrEmpty(expect))
            return false;

        // REGEX format
        if (expect.StartsWith("REGEX:", StringComparison.OrdinalIgnoreCase))
            return expect.Length > 6;

        // Comparison format
        if (Regex.IsMatch(expect, @"^(GE|LE|GT|LT|EQ|NE):-?\d+(\.\d+)?$", RegexOptions.IgnoreCase))
            return true;

        // Contains format
        if (Regex.IsMatch(expect, @"^(CONTAINS|STARTS|ENDS):.+$", RegexOptions.IgnoreCase))
            return true;

        // Literal format (anything not starting with known prefixes)
        return !Regex.IsMatch(expect, @"^(REGEX|GE|LE|GT|LT|EQ|NE|CONTAINS|STARTS|ENDS):", RegexOptions.IgnoreCase);
    }

    private string? GetElementPath(object? schemaObject)
    {
        // This would need more sophisticated implementation based on XSD validation context
        return null;
    }
}