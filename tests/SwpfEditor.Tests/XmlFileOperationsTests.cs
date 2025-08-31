using System.IO;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace SwpfEditor.Tests;

public class XmlFileOperationsTests
{
    [Fact]
    public void LoadXml_ValidFile_ShouldParseCorrectly()
    {
        // Arrange
        var xml = """
                  <?xml version="1.0" encoding="utf-8"?>
                  <test id="T1" alias="SampleTest">
                    <section id="S1" alias="Setup">
                      <step id="step1" alias="GetOsViaSsh" targetType="ssh" target="DUT_SSH" method="exec" timeout="30">
                        <extract name="os" pattern="(?i)Ubuntu" />
                        <check sourceRef="os" expect="Ubuntu" />
                      </step>
                    </section>
                  </test>
                  """;
        
        // Act
        var doc = XDocument.Parse(xml);
        
        // Assert
        Assert.NotNull(doc.Root);
        Assert.Equal("test", doc.Root.Name.LocalName);
        Assert.Equal("T1", doc.Root.Attribute("id")?.Value);
        Assert.Equal("SampleTest", doc.Root.Attribute("alias")?.Value);
        
        var section = doc.Root.Element("section");
        Assert.NotNull(section);
        Assert.Equal("S1", section.Attribute("id")?.Value);
        
        var step = section.Element("step");
        Assert.NotNull(step);
        Assert.Equal("step1", step.Attribute("id")?.Value);
        Assert.Equal("ssh", step.Attribute("targetType")?.Value);
    }

    [Fact]
    public void SaveXml_ShouldPreserveStructureAndEncoding()
    {
        // Arrange
        var doc = new XDocument(
            new XElement("test",
                new XAttribute("id", "T1"),
                new XAttribute("alias", "TestAlias"),
                new XElement("section",
                    new XAttribute("id", "S1"),
                    new XElement("step",
                        new XAttribute("id", "step1"),
                        new XAttribute("targetType", "ssh")))));

        // Act
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, new UTF8Encoding(false));
        doc.Save(writer, SaveOptions.None);
        writer.Flush();
        
        var result = Encoding.UTF8.GetString(ms.ToArray());
        
        // Assert
        Assert.Contains("<?xml version=\"1.0\" encoding=\"utf-8\"?>", result);
        Assert.Contains("<test id=\"T1\" alias=\"TestAlias\">", result);
        Assert.Contains("<section id=\"S1\">", result);
        Assert.Contains("<step id=\"step1\" targetType=\"ssh\"", result);
        
        // Verify it can be parsed back
        var reloaded = XDocument.Parse(result);
        Assert.Equal("T1", reloaded.Root?.Attribute("id")?.Value);
        Assert.Equal("TestAlias", reloaded.Root?.Attribute("alias")?.Value);
    }

    [Fact]
    public void ModifyAttribute_ShouldPreserveOrder()
    {
        // Arrange
        var doc = XDocument.Parse("<test id=\"T1\" alias=\"Original\"></test>");
        var root = doc.Root!;
        
        // Act - modify existing attribute
        root.Attribute("alias")!.Value = "Modified";
        
        // Assert - order should be preserved (id first, then alias)
        var attrs = root.Attributes().ToList();
        Assert.Equal(2, attrs.Count);
        Assert.Equal("id", attrs[0].Name.LocalName);
        Assert.Equal("alias", attrs[1].Name.LocalName);
        Assert.Equal("T1", attrs[0].Value);
        Assert.Equal("Modified", attrs[1].Value);
    }

    [Fact]
    public void LoadSampleFile_ShouldLoadSuccessfully()
    {
        // Arrange
        var samplePath = Path.Combine("..", "..", "..", "..", "..", "samples", "test.xml");
        
        // Skip test if sample file doesn't exist (CI scenarios)
        if (!File.Exists(samplePath))
        {
            return; // Skip test
        }
        
        // Act
        var doc = XDocument.Load(samplePath);
        
        // Assert
        Assert.NotNull(doc.Root);
        Assert.Equal("test", doc.Root.Name.LocalName);
        
        // Should have the expected structure from the sample
        Assert.Equal("T1", doc.Root.Attribute("id")?.Value);
        Assert.Equal("SampleTest", doc.Root.Attribute("alias")?.Value);
        
        var section = doc.Root.Element("section");
        Assert.NotNull(section);
        Assert.Equal("S1", section.Attribute("id")?.Value);
        Assert.Equal("Setup", section.Attribute("alias")?.Value);
        
        var step = section.Element("step");
        Assert.NotNull(step);
        Assert.Equal("step1", step.Attribute("id")?.Value);
        Assert.Equal("GetOsViaSsh", step.Attribute("alias")?.Value);
        Assert.Equal("ssh", step.Attribute("targetType")?.Value);
        Assert.Equal("DUT_SSH", step.Attribute("target")?.Value);
        
        var extract = step.Element("extract");
        Assert.NotNull(extract);
        Assert.Equal("os", extract.Attribute("name")?.Value);
        Assert.Equal("(?i)Ubuntu", extract.Attribute("pattern")?.Value);
        
        var check = step.Element("check");
        Assert.NotNull(check);
        Assert.Equal("os", check.Attribute("sourceRef")?.Value);
        Assert.Equal("Ubuntu", check.Attribute("expect")?.Value);
    }
}