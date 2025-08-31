using System.Xml.Linq;
using SwpfEditor.App.Models;
using Xunit;

namespace SwpfEditor.Tests
{
    public class XmlCommandsTests
    {
        [Fact]
        public void ChangeAttributeCommand_Execute_ChangesAttribute()
        {
            // Arrange
            var element = new XElement("test", new XAttribute("id", "original"));
            var command = new ChangeAttributeCommand(element, "id", "changed");

            // Act
            command.Execute();

            // Assert
            Assert.Equal("changed", element.Attribute("id")?.Value);
        }

        [Fact]
        public void ChangeAttributeCommand_Undo_RestoresOriginalValue()
        {
            // Arrange
            var element = new XElement("test", new XAttribute("id", "original"));
            var command = new ChangeAttributeCommand(element, "id", "changed");

            // Act
            command.Execute();
            command.Undo();

            // Assert
            Assert.Equal("original", element.Attribute("id")?.Value);
        }

        [Fact]
        public void AddElementCommand_Execute_AddsElement()
        {
            // Arrange
            var parent = new XElement("parent");
            var child = new XElement("child");
            var command = new AddElementCommand(parent, child);

            // Act
            command.Execute();

            // Assert
            Assert.Contains(child, parent.Elements());
        }

        [Fact]
        public void AddElementCommand_Undo_RemovesElement()
        {
            // Arrange
            var parent = new XElement("parent");
            var child = new XElement("child");
            var command = new AddElementCommand(parent, child);

            // Act
            command.Execute();
            command.Undo();

            // Assert
            Assert.DoesNotContain(child, parent.Elements());
        }

        [Fact]
        public void RemoveElementCommand_Execute_RemovesElement()
        {
            // Arrange
            var parent = new XElement("parent");
            var child = new XElement("child");
            parent.Add(child);
            var command = new RemoveElementCommand(child);

            // Act
            command.Execute();

            // Assert
            Assert.DoesNotContain(child, parent.Elements());
        }

        [Fact]
        public void RemoveElementCommand_Undo_RestoresElement()
        {
            // Arrange
            var parent = new XElement("parent");
            var child = new XElement("child");
            parent.Add(child);
            var command = new RemoveElementCommand(child);

            // Act
            command.Execute();
            command.Undo();

            // Assert
            Assert.Contains(child, parent.Elements());
        }

        [Fact]
        public void MoveElementCommand_Execute_MovesElement()
        {
            // Arrange
            var oldParent = new XElement("oldParent");
            var newParent = new XElement("newParent");
            var element = new XElement("element");
            oldParent.Add(element);
            var command = new MoveElementCommand(element, newParent);

            // Act
            command.Execute();

            // Assert
            Assert.DoesNotContain(element, oldParent.Elements());
            Assert.Contains(element, newParent.Elements());
        }

        [Fact]
        public void MoveElementCommand_Undo_RestoresOriginalPosition()
        {
            // Arrange
            var oldParent = new XElement("oldParent");
            var newParent = new XElement("newParent");
            var element = new XElement("element");
            oldParent.Add(element);
            var command = new MoveElementCommand(element, newParent);

            // Act
            command.Execute();
            command.Undo();

            // Assert
            Assert.Contains(element, oldParent.Elements());
            Assert.DoesNotContain(element, newParent.Elements());
        }

        [Fact]
        public void MoveElementCommand_WithInsertAfter_InsertsAtCorrectPosition()
        {
            // Arrange
            var parent = new XElement("parent");
            var existing1 = new XElement("existing1");
            var existing2 = new XElement("existing2");
            var moving = new XElement("moving");
            
            parent.Add(existing1, existing2);
            
            var otherParent = new XElement("other");
            otherParent.Add(moving);
            
            var command = new MoveElementCommand(moving, parent, existing1);

            // Act
            command.Execute();

            // Assert
            var elements = parent.Elements().ToList();
            Assert.Equal(3, elements.Count);
            Assert.Equal("existing1", elements[0].Name.LocalName);
            Assert.Equal("moving", elements[1].Name.LocalName);
            Assert.Equal("existing2", elements[2].Name.LocalName);
        }
    }
}