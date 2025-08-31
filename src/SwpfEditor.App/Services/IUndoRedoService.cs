using System;

namespace SwpfEditor.App.Services
{
    /// <summary>
    /// Interface for undo/redo operations
    /// </summary>
    public interface IUndoRedoService
    {
        /// <summary>
        /// Maximum number of undo operations to maintain
        /// </summary>
        int MaxUndoSteps { get; set; }

        /// <summary>
        /// Whether undo operation is available
        /// </summary>
        bool CanUndo { get; }

        /// <summary>
        /// Whether redo operation is available
        /// </summary>
        bool CanRedo { get; }

        /// <summary>
        /// Execute a command and add it to undo stack
        /// </summary>
        void ExecuteCommand(IUndoableCommand command);

        /// <summary>
        /// Undo the last operation
        /// </summary>
        void Undo();

        /// <summary>
        /// Redo the next operation
        /// </summary>
        void Redo();

        /// <summary>
        /// Clear all undo/redo history
        /// </summary>
        void Clear();

        /// <summary>
        /// Event fired when undo/redo state changes
        /// </summary>
        event EventHandler? StateChanged;
    }

    /// <summary>
    /// Interface for undoable commands
    /// </summary>
    public interface IUndoableCommand
    {
        /// <summary>
        /// Description of the command for UI display
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Execute the command
        /// </summary>
        void Execute();

        /// <summary>
        /// Undo the command
        /// </summary>
        void Undo();
    }
}