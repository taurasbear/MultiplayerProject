using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiplayerProject.Source.Memento
{
    /// <summary>
    /// Caretaker: Manages the history of CommandMemento objects.
    /// Provides undo functionality and command history tracking.
    /// Separates command log (all commands executed) from undo stack (restorable mementos).
    /// </summary>
    public class CommandHistoryManager
    {
        private readonly Stack<CommandMemento> _undoStack;
        private readonly List<string> _commandLog;
        private readonly int _maxHistorySize;

        public int UndoCount => _undoStack.Count;
        public bool CanUndo => _undoStack.Count > 0;

        public CommandHistoryManager(int maxHistorySize = 20)
        {
            _maxHistorySize = maxHistorySize;
            _undoStack = new Stack<CommandMemento>();
            _commandLog = new List<string>();
        }

        /// <summary>
        /// Saves a memento to the undo stack.
        /// If max size is reached, removes the oldest entry.
        /// </summary>
        public void SaveMemento(CommandMemento memento)
        {
            if (memento == null)
                return;

            _undoStack.Push(memento);

            // Limit undo stack size by removing oldest entries
            if (_undoStack.Count > _maxHistorySize)
            {
                // Convert to list, remove oldest (bottom of stack), convert back
                var stackList = _undoStack.ToList();
                stackList.RemoveAt(stackList.Count - 1);
                
                _undoStack.Clear();
                // Re-add in reverse order to maintain stack order
                for (int i = stackList.Count - 1; i >= 0; i--)
                {
                    _undoStack.Push(stackList[i]);
                }
            }
        }

        /// <summary>
        /// Logs a command execution (for display in history).
        /// This is separate from the undo stack.
        /// </summary>
        public void LogCommand(string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                return;

            string logEntry = $"[{DateTime.Now:HH:mm:ss}] {commandText}";
            _commandLog.Add(logEntry);

            // Limit log size
            if (_commandLog.Count > _maxHistorySize * 2) // Keep more logs than undo stack
            {
                _commandLog.RemoveAt(0);
            }
        }

        /// <summary>
        /// Retrieves the most recent memento without removing it from undo stack.
        /// </summary>
        public CommandMemento PeekLastMemento()
        {
            return _undoStack.Count > 0 ? _undoStack.Peek() : null;
        }

        /// <summary>
        /// Retrieves and removes the most recent memento for undo operation.
        /// </summary>
        public CommandMemento PopLastMemento()
        {
            return _undoStack.Count > 0 ? _undoStack.Pop() : null;
        }

        /// <summary>
        /// Clears all command history and undo stack.
        /// </summary>
        public void ClearHistory()
        {
            _undoStack.Clear();
            _commandLog.Clear();
        }

        /// <summary> (command log).
        /// </summary>
        public string GetHistoryReport(int maxEntries = 10)
        {
            if (_commandLog.Count == 0)
                return "No command history available.";

            var sb = new StringBuilder();
            var entriesToShow = Math.Min(_commandLog.Count, maxEntries);
            sb.Append($"=== COMMAND HISTORY (Last {entriesToShow} of {_commandLog.Count}) ===|");

            // Get most recent commands
            var recentCommands = _commandLog.Skip(Math.Max(0, _commandLog.Count - maxEntries)).ToList();
            for (int i = 0; i < recentCommands.Count; i++)
            {
                sb.Append($"{i + 1}. {recentCommands[i]}|");
            }

            sb.Append($"|Undo stack: {_undoStack.Count} restorable state(s)|");

            return sb.ToString().TrimEnd('|');
        }

        /// <summary>
        /// Gets all mementos in chronological order (oldest first).
        /// </summary>
        public List<CommandMemento> GetAllMementos()
        {
            var mementos = _undoStack.ToList();
            mementos.Reverse(); // Reverse to get chronological order
            return mementos;
        }

        /// <summary>
        /// Checks if command history contains any entries.
        /// </summary>
        public bool HasHistory()
        {
            return _commandLog.Count > 0;
        }
    }
}
