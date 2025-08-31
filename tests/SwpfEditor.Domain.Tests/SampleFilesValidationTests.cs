using Xunit;
using SwpfEditor.Domain.Services;
using System.Xml.Linq;

namespace SwpfEditor.Domain.Tests;

public class SampleFilesValidationTests
{
    [Fact]
    public void SampleTestXml_ShouldValidateAgainstSchema()
    {
        // Arrange
        var samplePath = "/home/runner/work/swpfeditor/swpfeditor/samples/test.xml";
        var schemaPath = "/home/runner/work/swpfeditor/swpfeditor/schemas/test-schema-1.0.xsd";
        
        if (!File.Exists(samplePath))
        {
            // Skip test if sample file doesn't exist
            return;
        }

        var document = XDocument.Load(samplePath);
        var validationService = new ValidationService(new DualXmlCoordinator(new PlaceholderResolver()));

        // Act
        var results = validationService.ValidateAgainstXsd(document, schemaPath);

        // Assert
        Assert.True(results.IsValid, 
            $"Sample test.xml validation failed: {string.Join(", ", results.Items.Select(i => i.Message))}");
    }

    [Fact]
    public void SampleTestConfigurationXml_ShouldValidateAgainstSchema()
    {
        // Arrange
        var samplePath = "/home/runner/work/swpfeditor/swpfeditor/samples/TestConfiguration.xml";
        var schemaPath = "/home/runner/work/swpfeditor/swpfeditor/schemas/test-configuration-schema.xsd";
        
        if (!File.Exists(samplePath))
        {
            // Skip test if sample file doesn't exist
            return;
        }

        var document = XDocument.Load(samplePath);
        var validationService = new ValidationService(new DualXmlCoordinator(new PlaceholderResolver()));

        // Act
        var results = validationService.ValidateAgainstXsd(document, schemaPath);

        // Assert
        Assert.True(results.IsValid, 
            $"Sample TestConfiguration.xml validation failed: {string.Join(", ", results.Items.Select(i => i.Message))}");
    }

    [Fact]
    public void SampleFiles_ShouldWorkTogetherWithDualXmlCoordinator()
    {
        // Arrange
        var testPath = "/home/runner/work/swpfeditor/swpfeditor/samples/test.xml";
        var configPath = "/home/runner/work/swpfeditor/swpfeditor/samples/TestConfiguration.xml";
        
        if (!File.Exists(testPath) || !File.Exists(configPath))
        {
            return; // Skip if files don't exist
        }

        var xmlMappingService = new XmlMappingService();
        var configMappingService = new ConfigurationMappingService();
        var placeholderResolver = new PlaceholderResolver();
        var dualXmlCoordinator = new DualXmlCoordinator(placeholderResolver);
        var validationService = new ValidationService(dualXmlCoordinator);

        // Load the sample files
        var testDocument = XDocument.Load(testPath);
        var configDocument = XDocument.Load(configPath);
        
        var test = xmlMappingService.XmlToTest(testDocument);
        var config = configMappingService.XmlToConfiguration(configDocument);

        // Act
        dualXmlCoordinator.InitializePlaceholderResolver(config);
        var sessions = dualXmlCoordinator.GenerateSessionsFromConfiguration(test, config);
        var targetMappingErrors = dualXmlCoordinator.ValidateStepTargetMappings(test, config);
        var validationResults = validationService.ValidateTest(test, config);

        // Assert
        Assert.NotEmpty(sessions); // Should have generated sessions from config
        Assert.Empty(targetMappingErrors); // No target mapping errors
        Assert.True(validationResults.IsValid, 
            $"Sample files integration validation failed: {string.Join(", ", validationResults.Items.Where(i => i.Severity == Domain.Enums.ValidationSeverity.Error).Select(i => i.Message))}");
        
        // Assert DUT_SSH is mapped correctly
        var dutSshSession = sessions.FirstOrDefault(s => s.Name == "DUT_SSH");
        Assert.NotNull(dutSshSession);
        Assert.Equal(Domain.Enums.TargetType.Ssh, dutSshSession.Type);
    }
}