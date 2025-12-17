using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source.Memento
{
    /// <summary>
    /// Memento: Stores a snapshot of the game state before a command is executed.
    /// This class is immutable to prevent external modification of saved state.
    /// </summary>
    public class CommandMemento
    {
        // Immutable properties - state can only be set during construction
        public DateTime Timestamp { get; private set; }
        public string CommandText { get; private set; }
        public Dictionary<string, int> PlayerScores { get; private set; }
        public Dictionary<string, object> ContextVariables { get; private set; }

        public CommandMemento(string commandText, Dictionary<string, int> playerScores, Dictionary<string, object> contextVariables)
        {
            Timestamp = DateTime.Now;
            CommandText = commandText;
            
            // Deep copy to prevent external modifications
            PlayerScores = new Dictionary<string, int>(playerScores);
            ContextVariables = new Dictionary<string, object>(contextVariables);
        }

        public string GetDescription()
        {
            return $"[{Timestamp:HH:mm:ss}] {CommandText} (Players: {PlayerScores.Count})";
        }

        public override string ToString()
        {
            return GetDescription();
        }
    }
}
