using Xunit;
using SwpfEditor.Domain.Models;
using SwpfEditor.Domain.Services;
using SwpfEditor.Domain.Enums;

namespace SwpfEditor.Domain.Tests;

public class DualXmlCoordinatorTests
{
    [Fact]
    public void GenerateSessionsFromConfiguration_ShouldReturnExistingSessions()
    {
        // Arrange
        var placeholderResolver = new PlaceholderResolver();
        var coordinator = new DualXmlCoordinator(placeholderResolver);
        
        var test = new Test
        {
            Id = "T1",
            Sessions = new List<Session>
            {
                new() { Name = "ExistingSession", Type = TargetType.Ssh }
            }
        };
        
        var config = new TestConfiguration();

        // Act
        var sessions = coordinator.GenerateSessionsFromConfiguration(test, config);

        // Assert
        Assert.Single(sessions);
        Assert.Equal("ExistingSession", sessions[0].Name);
    }

    [Fact]
    public void GenerateSessionsFromConfiguration_ShouldGenerateFromConfig()
    {
        // Arrange
        var placeholderResolver = new PlaceholderResolver();
        var coordinator = new DualXmlCoordinator(placeholderResolver);
        
        var test = new Test { Id = "T1" }; // No existing sessions
        
        var config = new TestConfiguration
        {
            SshList = new List<SshConnection>
            {
                new() { Name = "DUT_SSH", Host = "192.168.1.100", Port = 22 }
            },
            HttpList = new List<HttpConnection>
            {
                new() { Name = "API_ENDPOINT", BaseUrl = "http://api.example.com" }
            }
        };

        // Act
        var sessions = coordinator.GenerateSessionsFromConfiguration(test, config);

        // Assert
        Assert.Equal(2, sessions.Count);
        
        var sshSession = sessions.First(s => s.Type == TargetType.Ssh);
        Assert.Equal("DUT_SSH", sshSession.Name);
        Assert.Equal("192.168.1.100", sshSession.Host);
        Assert.Equal(22, sshSession.Port);
        
        var httpSession = sessions.First(s => s.Type == TargetType.Http);
        Assert.Equal("API_ENDPOINT", httpSession.Name);
        Assert.Equal("http://api.example.com", httpSession.BaseUrl);
    }

    [Fact]
    public void ValidateStepTargetMappings_ShouldDetectMissingTarget()
    {
        // Arrange
        var placeholderResolver = new PlaceholderResolver();
        var coordinator = new DualXmlCoordinator(placeholderResolver);
        
        var test = new Test
        {
            Id = "T1",
            Steps = new List<Step>
            {
                new() { Id = "step1", Target = "MISSING_TARGET", TargetType = TargetType.Ssh, Timeout = 30 }
            }
        };
        
        var config = new TestConfiguration
        {
            SshList = new List<SshConnection>
            {
                new() { Name = "DUT_SSH", Host = "192.168.1.100", Port = 22 }
            }
        };

        // Act
        var errors = coordinator.ValidateStepTargetMappings(test, config);

        // Assert
        Assert.Single(errors);
        Assert.Contains("Step 'step1' references unknown target 'MISSING_TARGET'", errors[0]);
    }

    [Fact]
    public void ValidateStepTargetMappings_ShouldDetectTypeMismatch()
    {
        // Arrange
        var placeholderResolver = new PlaceholderResolver();
        var coordinator = new DualXmlCoordinator(placeholderResolver);
        
        var test = new Test
        {
            Id = "T1",
            Steps = new List<Step>
            {
                new() { Id = "step1", Target = "DUT_SSH", TargetType = TargetType.Http, Timeout = 30 } // Wrong type
            }
        };
        
        var config = new TestConfiguration
        {
            SshList = new List<SshConnection>
            {
                new() { Name = "DUT_SSH", Host = "192.168.1.100", Port = 22 }
            }
        };

        // Act
        var errors = coordinator.ValidateStepTargetMappings(test, config);

        // Assert
        Assert.Single(errors);
        Assert.Contains("Step 'step1' target 'DUT_SSH' type mismatch: expected Http, found Ssh", errors[0]);
    }

    [Fact]
    public void ValidateStepTargetMappings_ShouldPassValidMappings()
    {
        // Arrange
        var placeholderResolver = new PlaceholderResolver();
        var coordinator = new DualXmlCoordinator(placeholderResolver);
        
        var test = new Test
        {
            Id = "T1",
            Steps = new List<Step>
            {
                new() { Id = "step1", Target = "DUT_SSH", TargetType = TargetType.Ssh, Timeout = 30 },
                new() { Id = "step2", Target = "API_ENDPOINT", TargetType = TargetType.Http, Timeout = 30 }
            }
        };
        
        var config = new TestConfiguration
        {
            SshList = new List<SshConnection>
            {
                new() { Name = "DUT_SSH", Host = "192.168.1.100", Port = 22 }
            },
            HttpList = new List<HttpConnection>
            {
                new() { Name = "API_ENDPOINT", BaseUrl = "http://api.example.com" }
            }
        };

        // Act
        var errors = coordinator.ValidateStepTargetMappings(test, config);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void InitializePlaceholderResolver_ShouldLoadAllConfigurationValues()
    {
        // Arrange
        var placeholderResolver = new PlaceholderResolver();
        var coordinator = new DualXmlCoordinator(placeholderResolver);
        
        var config = new TestConfiguration
        {
            LogDirectory = "/test/logs",
            UUT = new UUT
            {
                Properties = new Dictionary<string, object>
                {
                    { "SerialNumber", "12345" },
                    { "Model", "TestDevice" }
                }
            },
            Inputs = new List<Input>
            {
                new() { Name = "USER_INPUT", Value = "test_user" }
            },
            Variables = new List<Variable>
            {
                new() { Name = "TEST_VAR", Value = "test_value" }
            },
            Constants = new List<Constant>
            {
                new() { Name = "TEST_CONST", Value = "const_value" }
            }
        };

        // Act
        coordinator.InitializePlaceholderResolver(config);

        // Assert
        Assert.Equal("test_user", placeholderResolver.ResolvePlaceholder("USER_INPUT"));
        Assert.Equal("test_value", placeholderResolver.ResolvePlaceholder("TEST_VAR"));
        Assert.Equal("const_value", placeholderResolver.ResolvePlaceholder("TEST_CONST"));
        Assert.Equal("/test/logs", placeholderResolver.ResolvePlaceholder("LogDirectory"));
        Assert.Equal("12345", placeholderResolver.ResolvePlaceholder("UUT.SerialNumber"));
        Assert.Equal("TestDevice", placeholderResolver.ResolvePlaceholder("UUT.Model"));
    }

    [Fact]
    public void ValidatePlaceholders_ShouldDetectUnresolvedPlaceholders()
    {
        // Arrange
        var placeholderResolver = new PlaceholderResolver();
        placeholderResolver.SetConstant("KNOWN_VAR", "known_value");
        
        var coordinator = new DualXmlCoordinator(placeholderResolver);
        
        var test = new Test
        {
            Id = "T1",
            Steps = new List<Step>
            {
                new()
                {
                    Id = "step1",
                    Target = "${KNOWN_VAR}",
                    TargetType = TargetType.Ssh,
                    Timeout = 30,
                    Command = "echo ${UNKNOWN_VAR}",
                    Extracts = new List<Extract>
                    {
                        new()
                        {
                            Name = "extract1",
                            Pattern = "${ANOTHER_UNKNOWN}"
                        }
                    }
                }
            }
        };

        // Act
        var results = coordinator.ValidatePlaceholders(test);

        // Assert
        Assert.Equal(2, results.Count); // Two unresolved placeholders
        Assert.Contains(results, r => r.Message.Contains("UNKNOWN_VAR"));
        Assert.Contains(results, r => r.Message.Contains("ANOTHER_UNKNOWN"));
        Assert.All(results, r => Assert.Equal(ValidationSeverity.Error, r.Severity));
    }
}