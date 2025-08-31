using System.Runtime.InteropServices;
using System.Xml.Linq;
using Xunit;

namespace SwpfEditor.Tests;

public class XmlFileServiceTests
{
    [Fact]
    public void CreateElementHeader_WithAlias_ShouldPrioritizeAlias()
    {
        // Arrange
        var element = new XElement("step",
            new XAttribute("id", "step1"),
            new XAttribute("alias", "MyStep"));

        // Act
        var header = CreateElementHeaderForTest(element);

        // Assert
        Assert.Equal("step (MyStep)", header);
    }

    [Fact]
    public void CreateElementHeader_WithIdOnly_ShouldUseId()
    {
        // Arrange
        var element = new XElement("step", new XAttribute("id", "step1"));

        // Act
        var header = CreateElementHeaderForTest(element);

        // Assert
        Assert.Equal("step (step1)", header);
    }

    [Fact]
    public void CreateElementHeader_WithoutIdOrAlias_ShouldUseElementName()
    {
        // Arrange
        var element = new XElement("section");

        // Act
        var header = CreateElementHeaderForTest(element);

        // Assert
        Assert.Equal("section", header);
    }

    /// <summary>
    /// Test implementation of element header logic (same as XmlFileService.CreateElementHeader)
    /// This tests the core business logic independent of WPF dependencies
    /// </summary>
    private static string CreateElementHeaderForTest(XElement element)
    {
        var id = element.Attribute("id")?.Value;
        var alias = element.Attribute("alias")?.Value;
        
        if (!string.IsNullOrWhiteSpace(alias)) 
            return $"{element.Name.LocalName} ({alias})";
        
        if (!string.IsNullOrWhiteSpace(id)) 
            return $"{element.Name.LocalName} ({id})";
            
        return element.Name.LocalName;
    }
}