using Xunit;
using SwpfEditor.Domain.Models;
using SwpfEditor.Domain.Services;
using SwpfEditor.Domain.Enums;
using System.Xml.Linq;

namespace SwpfEditor.Domain.Tests;

/// <summary>
/// Integration tests for the acceptance criteria specified in the requirements
/// </summary>
public class AcceptanceCriteriaTests
{
    [Fact]
    public void GetOsViaSsh_ShouldMapDutSshAndValidateSuccessfully()
    {
        // Arrange - Create the test case from samples/test.xml
        var test = new Test
        {
            Id = "T1",
            Alias = "SampleTest",
            Sections = new List<Section>
            {
                new()
                {
                    Id = "S1",
                    Name = "Setup",
                    Refs = new List<Ref>
                    {
                        new() { Step = "step1", Mode = RefMode.Id }
                    }
                }
            },
            Steps = new List<Step>
            {
                new()
                {
                    Id = "step1",
                    Alias = "GetOsViaSsh",
                    TargetType = TargetType.Ssh,
                    Target = "DUT_SSH",
                    Timeout = 30,
                    Command = "uname -a",
                    Extracts = new List<Extract>
                    {
                        new()
                        {
                            Name = "os",
                            Pattern = "(?i)Ubuntu",
                            Checks = new List<Check>
                            {
                                new() { SourceRef = "os", Expect = "Ubuntu" }
                            }
                        }
                    }
                }
            }
        };

        // Arrange - Create configuration with DUT_SSH connection
        var config = new TestConfiguration
        {
            SshList = new List<SshConnection>
            {
                new()
                {
                    Name = "DUT_SSH",
                    Host = "192.168.1.100",
                    Port = 22,
                    Username = "admin",
                    Password = "secret123",
                    Prompt = "$"
                }
            },
            Constants = new List<Constant>
            {
                new() { Name = "EXPECTED_OS", Value = "Ubuntu" }
            }
        };

        // Arrange - Setup services
        var placeholderResolver = new PlaceholderResolver();
        var dualXmlCoordinator = new DualXmlCoordinator(placeholderResolver);
        var validationService = new ValidationService(dualXmlCoordinator);

        // Act - Initialize placeholder resolver and validate
        dualXmlCoordinator.InitializePlaceholderResolver(config);
        var sessions = dualXmlCoordinator.GenerateSessionsFromConfiguration(test, config);
        var targetMappingErrors = dualXmlCoordinator.ValidateStepTargetMappings(test, config);
        var validationResults = validationService.ValidateTest(test, config);

        // Assert - DUT_SSH should be automatically mapped
        Assert.Single(sessions, s => s.Name == "DUT_SSH" && s.Type == TargetType.Ssh);
        var dutSshSession = sessions.First(s => s.Name == "DUT_SSH");
        Assert.Equal("192.168.1.100", dutSshSession.Host);
        Assert.Equal(22, dutSshSession.Port);
        Assert.Equal("admin", dutSshSession.User);

        // Assert - No target mapping errors
        Assert.Empty(targetMappingErrors);

        // Assert - Test should validate successfully
        Assert.True(validationResults.IsValid, 
            $"Validation failed: {string.Join(", ", validationResults.Items.Where(i => i.Severity == ValidationSeverity.Error).Select(i => i.Message))}");

        // Assert - Step reference integrity should be valid
        Assert.DoesNotContain(validationResults.Items, 
            r => r.Message.Contains("references unknown step"));

        // Assert - Extract/check reference should be valid
        Assert.DoesNotContain(validationResults.Items, 
            r => r.Message.Contains("Check references unknown extract"));

        // Assert - Expect format should be valid
        Assert.DoesNotContain(validationResults.Items, 
            r => r.RuleName == "ExpectFormat");
    }

    [Fact]
    public void BrokenConfigurationConnection_ShouldProduceValidationError()
    {
        // Arrange - Test with DUT_SSH target
        var test = new Test
        {
            Id = "T1",
            Steps = new List<Step>
            {
                new()
                {
                    Id = "step1",
                    TargetType = TargetType.Ssh,
                    Target = "DUT_SSH",
                    Timeout = 30
                }
            }
        };

        // Arrange - Configuration with wrong name or missing connection
        var config = new TestConfiguration
        {
            SshList = new List<SshConnection>
            {
                new()
                {
                    Name = "WRONG_NAME", // Should be DUT_SSH
                    Host = "192.168.1.100",
                    Port = 22
                }
            }
        };

        // Arrange - Setup services
        var placeholderResolver = new PlaceholderResolver();
        var dualXmlCoordinator = new DualXmlCoordinator(placeholderResolver);
        var validationService = new ValidationService(dualXmlCoordinator);

        // Act
        var targetMappingErrors = dualXmlCoordinator.ValidateStepTargetMappings(test, config);
        var validationResults = validationService.ValidateTest(test, config);

        // Assert - Should detect missing target mapping
        Assert.NotEmpty(targetMappingErrors);
        Assert.Contains(targetMappingErrors, e => e.Contains("Step 'step1' references unknown target 'DUT_SSH'"));

        // Assert - Validation should fail with errors
        Assert.True(validationResults.HasErrors);
        Assert.Contains(validationResults.Items, 
            r => r.Severity == ValidationSeverity.Error && r.RuleName == "TargetMapping");
    }

    [Fact]
    public void MissingPlaceholder_ShouldProduceValidationErrorWithSuggestion()
    {
        // Arrange - Test with placeholder that won't be resolved
        var test = new Test
        {
            Id = "T1",
            Steps = new List<Step>
            {
                new()
                {
                    Id = "step1",
                    TargetType = TargetType.Ssh,
                    Target = "DUT_SSH",
                    Timeout = 30,
                    Command = "echo ${MISSING_PLACEHOLDER}"
                }
            }
        };

        var config = new TestConfiguration
        {
            SshList = new List<SshConnection>
            {
                new() { Name = "DUT_SSH", Host = "192.168.1.100", Port = 22 }
            }
            // Missing the placeholder definition
        };

        // Arrange - Setup services
        var placeholderResolver = new PlaceholderResolver();
        var dualXmlCoordinator = new DualXmlCoordinator(placeholderResolver);
        var validationService = new ValidationService(dualXmlCoordinator);

        // Act
        dualXmlCoordinator.InitializePlaceholderResolver(config);
        var validationResults = validationService.ValidateTest(test, config);

        // Assert - Should detect unresolved placeholder
        Assert.True(validationResults.HasErrors);
        var placeholderError = validationResults.Items.FirstOrDefault(
            r => r.Severity == ValidationSeverity.Error && 
                 r.RuleName == "PlaceholderResolution" &&
                 r.Message.Contains("MISSING_PLACEHOLDER"));
        
        Assert.NotNull(placeholderError);
        Assert.NotNull(placeholderError.SuggestedFix);
        Assert.Contains("Define 'MISSING_PLACEHOLDER'", placeholderError.SuggestedFix);
        Assert.Contains("inputs, variables, constants, or TestConfiguration", placeholderError.SuggestedFix);
    }

    [Fact]
    public void XmlRoundTripConversion_ShouldPreserveTestStructure()
    {
        // Arrange - Original test from samples
        var originalTest = new Test
        {
            Id = "T1",
            Alias = "SampleTest",
            Steps = new List<Step>
            {
                new()
                {
                    Id = "step1",
                    Alias = "GetOsViaSsh",
                    TargetType = TargetType.Ssh,
                    Target = "DUT_SSH",
                    Method = Enums.HttpMethod.GET, // This should be ignored for SSH
                    Timeout = 30,
                    Extracts = new List<Extract>
                    {
                        new()
                        {
                            Name = "os",
                            Pattern = "(?i)Ubuntu",
                            Checks = new List<Check>
                            {
                                new() { SourceRef = "os", Expect = "Ubuntu" }
                            }
                        }
                    }
                }
            },
            Sections = new List<Section>
            {
                new()
                {
                    Id = "S1",
                    Name = "Setup",
                    Refs = new List<Ref>
                    {
                        new() { Step = "step1", Mode = RefMode.Id }
                    }
                }
            }
        };

        // Arrange - Mapping service
        var mappingService = new XmlMappingService();

        // Act - Convert to XML and back
        var xmlDocument = mappingService.TestToXml(originalTest);
        var roundTripTest = mappingService.XmlToTest(xmlDocument);

        // Assert - Structure should be preserved
        Assert.Equal(originalTest.Id, roundTripTest.Id);
        Assert.Equal(originalTest.Alias, roundTripTest.Alias);
        Assert.Equal(originalTest.Steps.Count, roundTripTest.Steps.Count);
        Assert.Equal(originalTest.Sections.Count, roundTripTest.Sections.Count);

        // Assert - Step details preserved
        var originalStep = originalTest.Steps[0];
        var roundTripStep = roundTripTest.Steps[0];
        Assert.Equal(originalStep.Id, roundTripStep.Id);
        Assert.Equal(originalStep.Alias, roundTripStep.Alias);
        Assert.Equal(originalStep.Target, roundTripStep.Target);
        Assert.Equal(originalStep.TargetType, roundTripStep.TargetType);
        Assert.Equal(originalStep.Timeout, roundTripStep.Timeout);

        // Assert - Extract and check preserved
        Assert.Equal(originalStep.Extracts.Count, roundTripStep.Extracts.Count);
        var originalExtract = originalStep.Extracts[0];
        var roundTripExtract = roundTripStep.Extracts[0];
        Assert.Equal(originalExtract.Name, roundTripExtract.Name);
        Assert.Equal(originalExtract.Pattern, roundTripExtract.Pattern);
        Assert.Equal(originalExtract.Checks.Count, roundTripExtract.Checks.Count);
        Assert.Equal(originalExtract.Checks[0].Expect, roundTripExtract.Checks[0].Expect);
        Assert.Equal(originalExtract.Checks[0].SourceRef, roundTripExtract.Checks[0].SourceRef);
    }
}