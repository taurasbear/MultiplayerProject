using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MultiplayerProject.Source.Memento
{
    /// <summary>
    /// Memento: Stores a snapshot of the game state before a command is executed.
    /// This class is immutable and properly encapsulated - external code cannot modify the stored state.
    /// Only the Originator (GameCommandContext) can access the internal state through friend methods.
    /// </summary>
    public class CommandMemento
    {
        // Private fields - completely hidden from external access
        private readonly DateTime _timestamp;
        private readonly string _commandText;
        private readonly Dictionary<string, int> _playerScores;
        private readonly Dictionary<string, object> _contextVariables;

        // Public read-only properties that return copies or safe data
        public DateTime Timestamp => _timestamp;
        public string CommandText => _commandText;

        public CommandMemento(string commandText, Dictionary<string, int> playerScores, Dictionary<string, object> contextVariables)
        {
            _timestamp = DateTime.Now;
            _commandText = commandText;
            
            // Deep copy to prevent external modifications
            _playerScores = new Dictionary<string, int>(playerScores);
            _contextVariables = new Dictionary<string, object>(contextVariables);
        }

        // Friend method - only GameCommandContext (Originator) should use these
        // External classes cannot modify the dictionaries
        internal Dictionary<string, int> GetPlayerScoresForRestore()
        {
            // Return a copy to prevent modification of memento state
            return new Dictionary<string, int>(_playerScores);
        }

        internal Dictionary<string, object> GetContextVariablesForRestore()
        {
            // Return a copy to prevent modification of memento state
            return new Dictionary<string, object>(_contextVariables);
        }

        public string GetDescription()
        {
            return $"[{_timestamp:HH:mm:ss}] {_commandText} (Players: {_playerScores.Count})";
        }

        public override string ToString()
        {
            return GetDescription();
        }
    }
}
