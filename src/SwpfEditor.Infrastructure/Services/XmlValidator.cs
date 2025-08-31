using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using SwpfEditor.Domain.Models;
using SwpfEditor.Domain.Services;

namespace SwpfEditor.Infrastructure.Services;

public class XmlValidator : IXmlValidator
{
    private readonly IConnectionResolver _connectionResolver;
    private readonly IPlaceholderResolver _placeholderResolver;

    public XmlValidator(IConnectionResolver connectionResolver, IPlaceholderResolver placeholderResolver)
    {
        _connectionResolver = connectionResolver;
        _placeholderResolver = placeholderResolver;
    }

    public List<ValidationError> ValidateAgainstSchema(string xmlContent, string schemaPath)
    {
        var errors = new List<ValidationError>();

        try
        {
            var settings = new XmlReaderSettings();
            settings.Schemas.Add(null, schemaPath);
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += (sender, e) =>
            {
                errors.Add(new ValidationError
                {
                    Message = e.Message,
                    Severity = e.Severity == XmlSeverityType.Error ? ValidationSeverity.Error : ValidationSeverity.Warning,
                    LineNumber = e.Exception?.LineNumber
                });
            };

            using var reader = XmlReader.Create(new StringReader(xmlContent), settings);
            while (reader.Read()) { }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError
            {
                Message = $"Schema validation failed: {ex.Message}",
                Severity = ValidationSeverity.Error
            });
        }

        return errors;
    }

    public List<ValidationError> ValidateBusinessRules(Test test, TestConfiguration? testConfiguration = null)
    {
        var errors = new List<ValidationError>();

        // Validate XSD 1.1 rules that can't be expressed in XSD 1.0
        ValidateConditionalRequirements(test, errors);
        ValidateReferentialIntegrity(test, errors);
        ValidateExpectFormats(test, errors);

        // Validate connections if test configuration is provided
        if (testConfiguration != null)
        {
            errors.AddRange(_connectionResolver.ValidateConnections(test, testConfiguration));
        }

        return errors;
    }

    public List<ValidationError> ValidateComprehensive(string xmlContent, Test test, TestConfiguration? testConfiguration = null)
    {
        var errors = new List<ValidationError>();

        // 1. XSD 1.0 validation
        var xsdPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "schemas", "script-v1.xsd");
        if (File.Exists(xsdPath))
        {
            errors.AddRange(ValidateAgainstSchema(xmlContent, xsdPath));
        }

        // 2. Business rules validation
        errors.AddRange(ValidateBusinessRules(test, testConfiguration));

        // 3. Placeholder validation if test configuration is provided
        if (testConfiguration != null)
        {
            var context = PlaceholderContext.FromTestConfiguration(testConfiguration);
            ValidatePlaceholdersInTest(test, context, errors);
        }

        return errors;
    }

    private void ValidateConditionalRequirements(Test test, List<ValidationError> errors)
    {
        // Session conditional requirements
        if (test.Sessions?.SessionList != null)
        {
            foreach (var session in test.Sessions.SessionList)
            {
                if (session.Type == Domain.Models.TargetType.Ssh || session.Type == Domain.Models.TargetType.Telnet)
                {
                    if (string.IsNullOrEmpty(session.Host))
                    {
                        errors.Add(new ValidationError
                        {
                            Message = $"Session '{session.Name}' of type '{session.Type}' requires 'host' attribute",
                            Severity = ValidationSeverity.Error
                        });
                    }
                    if (!session.Port.HasValue)
                    {
                        errors.Add(new ValidationError
                        {
                            Message = $"Session '{session.Name}' of type '{session.Type}' requires 'port' attribute",
                            Severity = ValidationSeverity.Error
                        });
                    }
                }
                else if (session.Type == Domain.Models.TargetType.Http)
                {
                    if (string.IsNullOrEmpty(session.BaseUrl))
                    {
                        errors.Add(new ValidationError
                        {
                            Message = $"Session '{session.Name}' of type 'Http' requires 'baseUrl' attribute",
                            Severity = ValidationSeverity.Error
                        });
                    }
                }
            }
        }

        // Step conditional requirements
        ValidateStepsConditionalRequirements(test.Steps?.StepList, errors);
        if (test.Sections?.SectionList != null)
        {
            foreach (var section in test.Sections.SectionList)
            {
                ValidateStepsConditionalRequirements(section.Steps, errors);
            }
        }
    }

    private void ValidateStepsConditionalRequirements(List<Step>? steps, List<ValidationError> errors)
    {
        if (steps == null) return;

        foreach (var step in steps)
        {
            if (step.TargetType == Domain.Models.TargetType.Http && step.Method == null)
            {
                errors.Add(new ValidationError
                {
                    Message = $"Step '{step.Id}' with targetType 'http' requires 'method' attribute",
                    ElementId = step.Id,
                    PropertyName = "method",
                    Severity = ValidationSeverity.Error
                });
            }

            if (step.Timeout.HasValue && step.Timeout < 0)
            {
                errors.Add(new ValidationError
                {
                    Message = $"Step '{step.Id}' timeout must be >= 0",
                    ElementId = step.Id,
                    PropertyName = "timeout",
                    Severity = ValidationSeverity.Error
                });
            }
        }
    }

    private void ValidateReferentialIntegrity(Test test, List<ValidationError> errors)
    {
        var allStepIds = new HashSet<string>();
        var allExtractNames = new Dictionary<string, HashSet<string>>(); // stepId -> extract names

        // Collect all step IDs and extract names
        CollectStepReferences(test.Steps?.StepList, allStepIds, allExtractNames);
        if (test.Sections?.SectionList != null)
        {
            foreach (var section in test.Sections.SectionList)
            {
                CollectStepReferences(section.Steps, allStepIds, allExtractNames);
            }
        }

        // Validate check sourceRef references
        ValidateCheckReferences(test.Steps?.StepList, allExtractNames, errors);
        if (test.Sections?.SectionList != null)
        {
            foreach (var section in test.Sections.SectionList)
            {
                ValidateCheckReferences(section.Steps, allExtractNames, errors);
            }
        }

        // Validate section refs
        if (test.Sections?.SectionList != null)
        {
            foreach (var section in test.Sections.SectionList)
            {
                if (section.Refs?.RefList != null)
                {
                    foreach (var refItem in section.Refs.RefList)
                    {
                        if (!allStepIds.Contains(refItem.Step))
                        {
                            errors.Add(new ValidationError
                            {
                                Message = $"Reference '{refItem.Step}' in section '{section.Id}' points to non-existent step",
                                ElementId = section.Id,
                                Severity = ValidationSeverity.Error
                            });
                        }
                    }
                }
            }
        }
    }

    private void ValidateExpectFormats(Test test, List<ValidationError> errors)
    {
        var expectPrefixPattern = new Regex(@"^(eq|ne|gt|gte|lt|lte|contains|startsWith|endsWith|regex|isEmpty|isNotEmpty):.+$");

        ValidateStepExpectFormats(test.Steps?.StepList, expectPrefixPattern, errors);
        if (test.Sections?.SectionList != null)
        {
            foreach (var section in test.Sections.SectionList)
            {
                ValidateStepExpectFormats(section.Steps, expectPrefixPattern, errors);
            }
        }
    }

    private void ValidateStepExpectFormats(List<Step>? steps, Regex expectPrefixPattern, List<ValidationError> errors)
    {
        if (steps == null) return;

        foreach (var step in steps)
        {
            if (step.Extracts?.ExtractList != null)
            {
                foreach (var extract in step.Extracts.ExtractList)
                {
                    if (extract.Checks?.CheckList != null)
                    {
                        foreach (var check in extract.Checks.CheckList)
                        {
                            if (!string.IsNullOrEmpty(check.Expect) && 
                                check.Expect.Contains(':') && 
                                !expectPrefixPattern.IsMatch(check.Expect))
                            {
                                errors.Add(new ValidationError
                                {
                                    Message = $"Invalid expect format '{check.Expect}' in check for extract '{extract.Name}' in step '{step.Id}'",
                                    ElementId = step.Id,
                                    Severity = ValidationSeverity.Warning
                                });
                            }
                        }
                    }
                }
            }
        }
    }

    private void CollectStepReferences(List<Step>? steps, HashSet<string> allStepIds, Dictionary<string, HashSet<string>> allExtractNames)
    {
        if (steps == null) return;

        foreach (var step in steps)
        {
            allStepIds.Add(step.Id);
            
            if (step.Extracts?.ExtractList != null)
            {
                var extractNames = new HashSet<string>();
                foreach (var extract in step.Extracts.ExtractList)
                {
                    extractNames.Add(extract.Name);
                }
                allExtractNames[step.Id] = extractNames;
            }
        }
    }

    private void ValidateCheckReferences(List<Step>? steps, Dictionary<string, HashSet<string>> allExtractNames, List<ValidationError> errors)
    {
        if (steps == null) return;

        foreach (var step in steps)
        {
            if (step.Extracts?.ExtractList != null)
            {
                foreach (var extract in step.Extracts.ExtractList)
                {
                    if (extract.Checks?.CheckList != null)
                    {
                        foreach (var check in extract.Checks.CheckList)
                        {
                            if (allExtractNames.TryGetValue(step.Id, out var stepExtractNames))
                            {
                                if (!stepExtractNames.Contains(check.SourceRef))
                                {
                                    errors.Add(new ValidationError
                                    {
                                        Message = $"Check sourceRef '{check.SourceRef}' in step '{step.Id}' references non-existent extract",
                                        ElementId = step.Id,
                                        Severity = ValidationSeverity.Error
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void ValidatePlaceholdersInTest(Test test, PlaceholderContext context, List<ValidationError> errors)
    {
        // Validate placeholders in step commands and other string properties
        ValidateStepPlaceholders(test.Steps?.StepList, context, errors);
        if (test.Sections?.SectionList != null)
        {
            foreach (var section in test.Sections.SectionList)
            {
                ValidateStepPlaceholders(section.Steps, context, errors);
            }
        }
    }

    private void ValidateStepPlaceholders(List<Step>? steps, PlaceholderContext context, List<ValidationError> errors)
    {
        if (steps == null) return;

        foreach (var step in steps)
        {
            if (!string.IsNullOrEmpty(step.Command))
            {
                var placeholderErrors = _placeholderResolver.ValidatePlaceholders(step.Command, context);
                foreach (var error in placeholderErrors)
                {
                    error.ElementId = step.Id;
                    error.PropertyName = "command";
                    errors.Add(error);
                }
            }

            // Validate placeholders in other string properties as needed
            if (!string.IsNullOrEmpty(step.Target))
            {
                var placeholderErrors = _placeholderResolver.ValidatePlaceholders(step.Target, context);
                foreach (var error in placeholderErrors)
                {
                    error.ElementId = step.Id;
                    error.PropertyName = "target";
                    errors.Add(error);
                }
            }
        }
    }
}