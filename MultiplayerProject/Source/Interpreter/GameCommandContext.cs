// File: GameCommandContext.cs
// Location: MultiplayerProject/Source/Interpreter/GameCommandContext.cs

using MultiplayerProject;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Context class that provides access to game state and operations for command interpretation.
    /// This encapsulates all the data and operations that commands might need to access.
    /// </summary>
    public class GameCommandContext
    {
        public Server Server { get; set; }
        public GameInstance CurrentGameInstance { get; set; }
        public List<ServerConnection> Connections { get; set; }
        public Dictionary<string, object> Variables { get; set; }

        public GameCommandContext()
        {
            Variables = new Dictionary<string, object>();
            Connections = new List<ServerConnection>();
        }

        public GameCommandContext(Server server) : this()
        {
            Server = server;
            if (server != null)
            {
                // Get connections from server
                Connections.AddRange(server.ComponentClients);
            }
        }

        /// <summary>
        /// Find a player connection by name
        /// </summary>
        public ServerConnection FindPlayerByName(string playerName)
        {
            foreach (var connection in Connections)
            {
                if (connection.Name.Equals(playerName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return connection;
                }
            }
            return null;
        }

        /// <summary>
        /// Get or set a context variable
        /// </summary>
        public T GetVariable<T>(string name, T defaultValue = default(T))
        {
            if (Variables.ContainsKey(name) && Variables[name] is T)
            {
                return (T)Variables[name];
            }
            return defaultValue;
        }

        public void SetVariable<T>(string name, T value)
        {
            Variables[name] = value;
        }

        /// <summary>
        /// Update connections list from server
        /// </summary>
        public void RefreshConnections()
        {
            if (Server != null)
            {
                Connections.Clear();
                Connections.AddRange(Server.ComponentClients);
            }
        }

        /// <summary>
        /// Get player score from game instance if available
        /// </summary>
        public int GetPlayerScore(string playerId)
        {
            // This would need to access GameInstance's private _playerScores
            // For now, return from variables or 0
            return GetVariable($"score_{playerId}", 0);
        }

        /// <summary>
        /// Set player score (would need GameInstance integration)
        /// </summary>
        public bool SetPlayerScore(string playerId, int score)
        {
            // Store in variables for now
            SetVariable($"score_{playerId}", score);
            return true; // In full implementation, would return success/failure
        }
    }
}