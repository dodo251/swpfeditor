using System.IO;
using System.Text;
using System.Xml.Linq;
using Xunit;

#if WINDOWS
using SwpfEditor.App.Services;
#endif

namespace SwpfEditor.Tests;

public class XmlFileServiceTests
{
    [Fact]
    public void CreateElementHeader_WithAlias_ShouldPrioritizeAlias()
    {
        // Skip on non-Windows as service is in WPF project
#if !WINDOWS
        return;
#else
        // Arrange
        var element = new XElement("step",
            new XAttribute("id", "step1"),
            new XAttribute("alias", "MyStep"));

        // Act
        var header = XmlFileService.CreateElementHeader(element);

        // Assert
        Assert.Equal("step (MyStep)", header);
#endif
    }

    [Fact]
    public void CreateElementHeader_WithIdOnly_ShouldUseId()
    {
        // Skip on non-Windows as service is in WPF project
#if !WINDOWS
        return;
#else
        // Arrange
        var element = new XElement("step", new XAttribute("id", "step1"));

        // Act
        var header = XmlFileService.CreateElementHeader(element);

        // Assert
        Assert.Equal("step (step1)", header);
#endif
    }

    [Fact]
    public void CreateElementHeader_WithoutIdOrAlias_ShouldUseElementName()
    {
        // Skip on non-Windows as service is in WPF project
#if !WINDOWS
        return;
#else
        // Arrange
        var element = new XElement("section");

        // Act
        var header = XmlFileService.CreateElementHeader(element);

        // Assert
        Assert.Equal("section", header);
#endif
    }
}