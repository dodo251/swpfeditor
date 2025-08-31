using SwpfEditor.App.Services;
using Xunit;

namespace SwpfEditor.Tests
{
    public class TemplateServiceTests
    {
        [Fact]
        public void TemplateService_LoadDefaultTemplates_CreatesBasicTemplates()
        {
            // Arrange
            var service = new TemplateService();

            // Act
            service.LoadTemplates("non-existent-file.xml"); // Should fall back to defaults

            // Assert
            Assert.True(service.IsLoaded);
            var templates = service.GetTemplates().ToList();
            Assert.NotEmpty(templates);
            
            // Check for basic templates
            Assert.Contains(templates, t => t.Name == "step");
            Assert.Contains(templates, t => t.Name == "section");
            Assert.Contains(templates, t => t.Name == "extract");
        }

        [Fact]
        public void TemplateService_CreateElementFromTemplate_GeneratesUniqueIds()
        {
            // Arrange
            var service = new TemplateService();
            service.LoadTemplates("non-existent-file.xml");

            // Act
            var element1 = service.CreateElementFromTemplate("step");
            var element2 = service.CreateElementFromTemplate("step");

            // Assert
            Assert.NotEqual(element1.Attribute("id")?.Value, element2.Attribute("id")?.Value);
        }

        [Fact]
        public void TemplateService_GetTemplatesForParent_FiltersCorrectly()
        {
            // Arrange
            var service = new TemplateService();
            service.LoadTemplates("non-existent-file.xml");

            // Act
            var stepTemplates = service.GetTemplatesForParent("steps").ToList();

            // Assert
            Assert.NotEmpty(stepTemplates);
            Assert.Contains(stepTemplates, t => t.Name == "step");
        }

        [Fact]
        public void TemplateService_CreateElementFromTemplate_ThrowsForUnknownTemplate()
        {
            // Arrange
            var service = new TemplateService();
            service.LoadTemplates("non-existent-file.xml");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => service.CreateElementFromTemplate("unknown-template"));
        }

        [Fact]
        public void TemplateService_CreateElementFromTemplate_ReturnsDeepCopy()
        {
            // Arrange
            var service = new TemplateService();
            service.LoadTemplates("non-existent-file.xml");

            // Act
            var element1 = service.CreateElementFromTemplate("step");
            var element2 = service.CreateElementFromTemplate("step");

            // Modify one element
            element1.SetAttributeValue("modified", "true");

            // Assert
            Assert.Null(element2.Attribute("modified"));
        }
    }
}