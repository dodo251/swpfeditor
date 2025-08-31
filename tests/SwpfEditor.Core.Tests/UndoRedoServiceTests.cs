using System.Xml.Linq;
using SwpfEditor.App.Services;
using Xunit;

namespace SwpfEditor.Tests
{
    public class UndoRedoServiceTests
    {
        [Fact]
        public void UndoRedoService_BasicOperations_WorkCorrectly()
        {
            // Arrange
            var service = new UndoRedoService();
            var testCommand = new TestCommand();
            
            // Act & Assert - Initial state
            Assert.False(service.CanUndo);
            Assert.False(service.CanRedo);
            
            // Execute command
            service.ExecuteCommand(testCommand);
            Assert.True(service.CanUndo);
            Assert.False(service.CanRedo);
            Assert.True(testCommand.WasExecuted);
            
            // Undo
            service.Undo();
            Assert.False(service.CanUndo);
            Assert.True(service.CanRedo);
            Assert.True(testCommand.WasUndone);
            
            // Redo
            service.Redo();
            Assert.True(service.CanUndo);
            Assert.False(service.CanRedo);
            Assert.Equal(2, testCommand.ExecuteCount);
        }

        [Fact]
        public void UndoRedoService_MaxSteps_LimitsCorrectly()
        {
            // Arrange
            var service = new UndoRedoService { MaxUndoSteps = 2 };
            
            // Act - Add more commands than max
            service.ExecuteCommand(new TestCommand { Description = "Command 1" });
            service.ExecuteCommand(new TestCommand { Description = "Command 2" });
            service.ExecuteCommand(new TestCommand { Description = "Command 3" });
            
            // Assert - Can only undo up to max steps
            int undoCount = 0;
            while (service.CanUndo)
            {
                service.Undo();
                undoCount++;
            }
            
            Assert.Equal(2, undoCount);
        }

        [Fact]
        public void UndoRedoService_StateChanged_EventFired()
        {
            // Arrange
            var service = new UndoRedoService();
            var eventFired = false;
            service.StateChanged += (s, e) => eventFired = true;
            
            // Act
            service.ExecuteCommand(new TestCommand());
            
            // Assert
            Assert.True(eventFired);
        }
    }

    // Test command implementation
    public class TestCommand : IUndoableCommand
    {
        public string Description { get; set; } = "Test Command";
        public bool WasExecuted { get; private set; }
        public bool WasUndone { get; private set; }
        public int ExecuteCount { get; private set; }

        public void Execute()
        {
            WasExecuted = true;
            ExecuteCount++;
        }

        public void Undo()
        {
            WasUndone = true;
        }
    }
}