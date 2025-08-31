using Xunit;
using SwpfEditor.Domain.Models;
using SwpfEditor.Domain.Services;
using SwpfEditor.Domain.Enums;

namespace SwpfEditor.Domain.Tests;

public class ValidationServiceTests
{
    [Fact]
    public void ValidateTest_ShouldDetectDuplicateStepIds()
    {
        // Arrange
        var dualXmlCoordinator = new DualXmlCoordinator(new PlaceholderResolver());
        var validationService = new ValidationService(dualXmlCoordinator);
        
        var test = new Test
        {
            Id = "T1",
            Steps = new List<Step>
            {
                new() { Id = "step1", Target = "DUT_SSH", TargetType = TargetType.Ssh, Timeout = 30 },
                new() { Id = "step1", Target = "DUT_SSH", TargetType = TargetType.Ssh, Timeout = 30 } // Duplicate ID
            }
        };

        // Act
        var results = validationService.ValidateTest(test);

        // Assert
        Assert.True(results.HasErrors);
        Assert.Contains(results.Items, r => r.Message.Contains("Duplicate step ID: 'step1'"));
    }

    [Fact]
    public void ValidateTest_ShouldDetectInvalidStepReferences()
    {
        // Arrange
        var dualXmlCoordinator = new DualXmlCoordinator(new PlaceholderResolver());
        var validationService = new ValidationService(dualXmlCoordinator);
        
        var test = new Test
        {
            Id = "T1",
            Steps = new List<Step>
            {
                new() { Id = "step1", Target = "DUT_SSH", TargetType = TargetType.Ssh, Timeout = 30 }
            },
            Sections = new List<Section>
            {
                new()
                {
                    Id = "section1",
                    Refs = new List<Ref>
                    {
                        new() { Step = "nonexistent_step", Mode = RefMode.Id }
                    }
                }
            }
        };

        // Act
        var results = validationService.ValidateTest(test);

        // Assert
        Assert.True(results.HasErrors);
        Assert.Contains(results.Items, r => r.Message.Contains("references unknown step ID 'nonexistent_step'"));
    }

    [Fact]
    public void ValidateTest_ShouldDetectInvalidCheckSourceRef()
    {
        // Arrange
        var dualXmlCoordinator = new DualXmlCoordinator(new PlaceholderResolver());
        var validationService = new ValidationService(dualXmlCoordinator);
        
        var test = new Test
        {
            Id = "T1",
            Steps = new List<Step>
            {
                new()
                {
                    Id = "step1",
                    Target = "DUT_SSH",
                    TargetType = TargetType.Ssh,
                    Timeout = 30,
                    Extracts = new List<Extract>
                    {
                        new()
                        {
                            Name = "extract1",
                            Pattern = "test",
                            Checks = new List<Check>
                            {
                                new() { SourceRef = "nonexistent_extract", Expect = "value" }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var results = validationService.ValidateTest(test);

        // Assert
        Assert.True(results.HasErrors);
        Assert.Contains(results.Items, r => r.Message.Contains("Check references unknown extract 'nonexistent_extract'"));
    }

    [Fact]
    public void ValidateTest_ShouldValidateHttpMethodRequired()
    {
        // Arrange
        var dualXmlCoordinator = new DualXmlCoordinator(new PlaceholderResolver());
        var validationService = new ValidationService(dualXmlCoordinator);
        
        var test = new Test
        {
            Id = "T1",
            Steps = new List<Step>
            {
                new()
                {
                    Id = "step1",
                    Target = "API_ENDPOINT",
                    TargetType = TargetType.Http,
                    Timeout = 30
                    // Missing Method for HTTP step
                }
            }
        };

        // Act
        var results = validationService.ValidateTest(test);

        // Assert
        Assert.True(results.HasErrors);
        Assert.Contains(results.Items, r => r.Message.Contains("HTTP step 'step1' must specify method"));
    }

    [Fact]
    public void ValidateTest_ShouldValidateRegexPatterns()
    {
        // Arrange
        var dualXmlCoordinator = new DualXmlCoordinator(new PlaceholderResolver());
        var validationService = new ValidationService(dualXmlCoordinator);
        
        var test = new Test
        {
            Id = "T1",
            Steps = new List<Step>
            {
                new()
                {
                    Id = "step1",
                    Target = "DUT_SSH",
                    TargetType = TargetType.Ssh,
                    Timeout = 30,
                    Extracts = new List<Extract>
                    {
                        new()
                        {
                            Name = "extract1",
                            Pattern = "[invalid regex" // Invalid regex pattern
                        }
                    }
                }
            }
        };

        // Act
        var results = validationService.ValidateTest(test);

        // Assert
        Assert.True(results.HasErrors);
        Assert.Contains(results.Items, r => r.Message.Contains("Extract 'extract1' pattern is not a valid regex"));
    }

    [Theory]
    [InlineData("REGEX:test", true)]
    [InlineData("GE:5", true)]
    [InlineData("LE:10.5", true)]
    [InlineData("EQ:-3", true)]
    [InlineData("CONTAINS:text", true)]
    [InlineData("STARTS:prefix", true)]
    [InlineData("ENDS:suffix", true)]
    [InlineData("literal text", true)]
    [InlineData("REGEX:", false)] // Empty pattern
    [InlineData("GE:not_a_number", false)]
    [InlineData("INVALID:format", true)] // Should be treated as literal
    public void ValidateTest_ShouldValidateExpectFormats(string expectValue, bool shouldBeValid)
    {
        // Arrange
        var dualXmlCoordinator = new DualXmlCoordinator(new PlaceholderResolver());
        var validationService = new ValidationService(dualXmlCoordinator);
        
        var test = new Test
        {
            Id = "T1",
            Steps = new List<Step>
            {
                new()
                {
                    Id = "step1",
                    Target = "DUT_SSH",
                    TargetType = TargetType.Ssh,
                    Timeout = 30,
                    Extracts = new List<Extract>
                    {
                        new()
                        {
                            Name = "extract1",
                            Pattern = "test",
                            Checks = new List<Check>
                            {
                                new() { SourceRef = "extract1", Expect = expectValue }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var results = validationService.ValidateTest(test);

        // Assert
        var hasExpectFormatError = results.Items.Any(r => r.RuleName == "ExpectFormat");
        Assert.Equal(!shouldBeValid, hasExpectFormatError);
    }
}