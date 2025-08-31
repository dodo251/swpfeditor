using System;
using System.Collections.Generic;
using System.Linq;

namespace SwpfEditor.App.Services
{
    /// <summary>
    /// Implementation of undo/redo service
    /// </summary>
    public class UndoRedoService : IUndoRedoService
    {
        private readonly Stack<IUndoableCommand> _undoStack = new();
        private readonly Stack<IUndoableCommand> _redoStack = new();
        private int _maxUndoSteps = 50; // Default to 50, requirement is ≥20

        public int MaxUndoSteps
        {
            get => _maxUndoSteps;
            set
            {
                _maxUndoSteps = Math.Max(1, value);
                TrimUndoStack();
                OnStateChanged();
            }
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public event EventHandler? StateChanged;

        public void ExecuteCommand(IUndoableCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            // Execute the command
            command.Execute();

            // Add to undo stack
            _undoStack.Push(command);

            // Clear redo stack since we're branching
            _redoStack.Clear();

            // Trim if needed
            TrimUndoStack();

            OnStateChanged();
        }

        public void Undo()
        {
            if (!CanUndo) return;

            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);

            OnStateChanged();
        }

        public void Redo()
        {
            if (!CanRedo) return;

            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);

            OnStateChanged();
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            OnStateChanged();
        }

        private void TrimUndoStack()
        {
            while (_undoStack.Count > _maxUndoSteps)
            {
                // Remove oldest commands (convert to array, reverse, skip, reverse back)
                var commands = _undoStack.ToArray();
                _undoStack.Clear();
                
                // Keep only the most recent MaxUndoSteps commands
                for (int i = commands.Length - _maxUndoSteps; i < commands.Length; i++)
                {
                    _undoStack.Push(commands[i]);
                }
            }
        }

        protected virtual void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}