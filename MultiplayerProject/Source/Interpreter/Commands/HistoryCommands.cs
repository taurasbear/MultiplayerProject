using MultiplayerProject.Source.Memento;
using System.Text;

namespace MultiplayerProject.Source.Commands
{
    /// <summary>
    /// Commands for managing command history and undo functionality.
    /// Uses the Memento pattern via CommandHistoryManager.
    /// </summary>
    public class HistoryCommand : ICommandExpression
    {
        public enum HistoryAction
        {
            Undo,
            ShowHistory,
            ClearHistory
        }

        private readonly HistoryAction _action;
        private readonly int _count;

        public HistoryCommand(HistoryAction action, int count = 10)
        {
            _action = action;
            _count = count;
        }

        public string Interpret(GameCommandContext context)
        {
            switch (_action)
            {
                case HistoryAction.Undo:
                    return UndoLastCommand(context);
                case HistoryAction.ShowHistory:
                    return ShowCommandHistory(context);
                case HistoryAction.ClearHistory:
                    return ClearCommandHistory(context);
                default:
                    return "Error: Unknown history action.";
            }
        }

        private string UndoLastCommand(GameCommandContext context)
        {
            // Get the CommandHistoryManager from context variables
            var historyManager = context.GetVariable<CommandHistoryManager>("__history_manager__");
            
            if (historyManager == null)
            {
                return "Error: Command history not available.";
            }

            if (!historyManager.CanUndo)
            {
                return "Nothing to undo. Undo stack is empty.";
            }

            // Pop the last memento
            var memento = historyManager.PopLastMemento();
            if (memento == null)
            {
                return "Error: Failed to retrieve last command state.";
            }

            // Restore the state from the memento (Originator restores itself)
            bool success = context.RestoreFromMemento(memento);
            
            if (success)
            {
                // Log the undo action
                historyManager.LogCommand($"/undo (reverted: {memento.CommandText})");
                
                return $"Successfully undone: {memento.CommandText}|" +
                       $"Restored to state from {memento.Timestamp:HH:mm:ss}|" +
                       $"Remaining undo steps: {historyManager.UndoCount}";
            }
            else
            {
                return "Error: Failed to restore game state from memento.";
            }
        }

        private string ShowCommandHistory(GameCommandContext context)
        {
            var historyManager = context.GetVariable<CommandHistoryManager>("__history_manager__");
            
            if (historyManager == null)
            {
                return "Error: Command history not available.";
            }

            return historyManager.GetHistoryReport(_count);
        }

        private string ClearCommandHistory(GameCommandContext context)
        {
            var historyManager = context.GetVariable<CommandHistoryManager>("__history_manager__");
            
            if (historyManager == null)
            {
                return "Error: Command history not available.";
            }

            int clearedUndoCount = historyManager.UndoCount;
            bool hadHistory = historyManager.HasHistory();
            historyManager.ClearHistory();
            
            // Log the clear action after clearing
            if (hadHistory || clearedUndoCount > 0)
            {
                historyManager.LogCommand("/clear_history (history was cleared)");
            }
            
            return $"Command history cleared. Removed {clearedUndoCount} undo state(s).";
        }
    }
}
