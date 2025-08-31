using Xunit;
using SwpfEditor.Domain.Services;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SwpfEditor.Domain.Tests;

public class XsdValidationTests
{
    [Fact]
    public void ValidateAgainstXsd10_ShouldPassForValidDocument()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<test id=""T1"" alias=""SampleTest"" xmlns=""http://sms.test/schema/1.0"">
  <steps>
    <step id=""step1"" alias=""GetOsViaSsh"" targetType=""ssh"" target=""DUT_SSH"" timeout=""30"">
      <extracts>
        <extract name=""os"" pattern=""(?i)Ubuntu"">
          <checks>
            <check sourceRef=""os"" expect=""Ubuntu"" />
          </checks>
        </extract>
      </extracts>
    </step>
  </steps>
  <sections>
    <section id=""S1"" name=""Setup"">
      <refs>
        <ref step=""step1"" />
      </refs>
    </section>
  </sections>
</test>";

        var document = XDocument.Parse(xmlContent);
        var validationService = new ValidationService(new DualXmlCoordinator(new PlaceholderResolver()));
        var schemaPath = "/home/runner/work/swpfeditor/swpfeditor/schemas/test-schema-1.0.xsd";

        // Act
        var results = validationService.ValidateAgainstXsd(document, schemaPath);

        // Assert
        Assert.True(results.IsValid, 
            $"XSD validation failed: {string.Join(", ", results.Items.Select(i => i.Message))}");
    }

    [Fact]
    public void ValidateAgainstXsd10_ShouldFailForInvalidStructure()
    {
        // Arrange - Invalid XML with check outside of extract
        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<test id=""T1"" xmlns=""http://sms.test/schema/1.0"">
  <check sourceRef=""invalid"" expect=""value"" />
</test>";

        var document = XDocument.Parse(xmlContent);
        var validationService = new ValidationService(new DualXmlCoordinator(new PlaceholderResolver()));
        var schemaPath = "/home/runner/work/swpfeditor/swpfeditor/schemas/test-schema-1.0.xsd";

        // Act
        var results = validationService.ValidateAgainstXsd(document, schemaPath);

        // Assert
        Assert.True(results.HasErrors);
        Assert.Contains(results.Items, r => r.RuleName == "XSD");
    }

    [Fact]
    public void TestConfigurationXsd_ShouldValidateCorrectly()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TestConfiguration xmlns=""http://sms.test/config/1.0"">
  <UUT>
    <SerialNumber>12345678ABCDEF01</SerialNumber>
    <Model>TestDevice-X1</Model>
  </UUT>
  <LogDirectory>C:\TestLogs</LogDirectory>
  <SshList>
    <SshConnection Name=""DUT_SSH"" Host=""192.168.1.100"" Port=""22"" Username=""admin"" Password=""secret123"" />
  </SshList>
  <Constants>
    <Constant Name=""EXPECTED_OS"" Value=""Ubuntu"" />
  </Constants>
</TestConfiguration>";

        var document = XDocument.Parse(xmlContent);
        var validationService = new ValidationService(new DualXmlCoordinator(new PlaceholderResolver()));
        var schemaPath = "/home/runner/work/swpfeditor/swpfeditor/schemas/test-configuration-schema.xsd";

        // Act
        var results = validationService.ValidateAgainstXsd(document, schemaPath);

        // Assert
        Assert.True(results.IsValid, 
            $"TestConfiguration XSD validation failed: {string.Join(", ", results.Items.Select(i => i.Message))}");
    }
}