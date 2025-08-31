using SwpfEditor.App.Validation;
using Xunit;

namespace SwpfEditor.Tests
{
    public class ValidationServiceTests
    {
        private readonly ValidationService _validationService = new();

        [Theory]
        [InlineData("test", "steps", true)]
        [InlineData("test", "sections", true)]
        [InlineData("test", "sessions", true)]
        [InlineData("steps", "step", true)]
        [InlineData("step", "extracts", true)]
        [InlineData("extracts", "extract", true)]
        [InlineData("extract", "checks", true)]
        [InlineData("checks", "check", true)]
        [InlineData("sections", "section", true)]
        [InlineData("section", "refs", true)]
        public void IsValidChild_ValidRelationships_ReturnsTrue(string parent, string child, bool expected)
        {
            // Act
            var result = _validationService.IsValidChild(parent, child);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("step", "section", false)]
        [InlineData("section", "step", false)]
        [InlineData("extract", "step", false)]
        [InlineData("check", "extract", false)]
        [InlineData("steps", "section", false)]
        public void IsValidChild_InvalidRelationships_ReturnsFalse(string parent, string child, bool expected)
        {
            // Act
            var result = _validationService.IsValidChild(parent, child);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetAllowedChildren_TestElement_ReturnsCorrectChildren()
        {
            // Act
            var allowedChildren = _validationService.GetAllowedChildren("test");

            // Assert
            Assert.Contains("meta", allowedChildren);
            Assert.Contains("description", allowedChildren);
            Assert.Contains("steps", allowedChildren);
            Assert.Contains("sections", allowedChildren);
            Assert.Contains("sessions", allowedChildren);
            Assert.DoesNotContain("step", allowedChildren);
        }

        [Fact]
        public void GetValidationMessage_InvalidRelationship_ReturnsMessage()
        {
            // Act
            var message = _validationService.GetValidationMessage("step", "section");

            // Assert
            Assert.NotNull(message);
            Assert.Contains("step", message);
            Assert.Contains("section", message);
            Assert.Contains("cannot contain", message);
        }

        [Fact]
        public void GetValidationMessage_ValidRelationship_ReturnsNull()
        {
            // Act
            var message = _validationService.GetValidationMessage("steps", "step");

            // Assert
            Assert.Null(message);
        }

        [Fact]
        public void IsValidChild_CaseInsensitive_WorksCorrectly()
        {
            // Act & Assert
            Assert.True(_validationService.IsValidChild("TEST", "steps"));
            Assert.True(_validationService.IsValidChild("Steps", "STEP"));
            Assert.True(_validationService.IsValidChild("EXTRACTS", "extract"));
        }

        [Fact]
        public void GetAllowedChildren_UnknownParent_ReturnsEmpty()
        {
            // Act
            var allowedChildren = _validationService.GetAllowedChildren("unknownElement");

            // Assert
            Assert.Empty(allowedChildren);
        }
    }
}