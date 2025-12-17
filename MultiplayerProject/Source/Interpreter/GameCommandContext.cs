using MultiplayerProject;
using MultiplayerProject.Source.Memento;
using System.Collections.Generic;
using System.Reflection;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Originator: Creates and restores from CommandMemento objects.
    /// This class holds the game state and can create snapshots of it.
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
                Connections.AddRange(server.ComponentClients);
            }
        }

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

        public void RefreshConnections()
        {
            if (Server != null)
            {
                Connections.Clear();
                Connections.AddRange(Server.ComponentClients);
            }
        }

        public int GetPlayerScore(string playerId)
        {
            return GetVariable($"score_{playerId}", 0);
        }
        public bool SetPlayerScore(string playerId, int score)
        {
            SetVariable($"score_{playerId}", score);
            return true; 
        }

        // ===== MEMENTO PATTERN METHODS (Originator) =====

        /// <summary>
        /// Creates a memento containing a snapshot of the current game state.
        /// This is called before executing state-changing commands.
        /// </summary>
        public CommandMemento CreateMemento(string commandText)
        {
            // Collect all player scores from Variables
            var playerScores = new Dictionary<string, int>();
            
            // Get scores from context variables
            foreach (var kvp in Variables)
            {
                if (kvp.Key.StartsWith("score_") && kvp.Value is int)
                {
                    playerScores[kvp.Key.Replace("score_", "")] = (int)kvp.Value;
                }
            }

            // Also get scores from GameInstance if available
            if (CurrentGameInstance != null)
            {
                try
                {
                    var playerScoresField = CurrentGameInstance.GetType().GetField("_playerScores", 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (playerScoresField != null)
                    {
                        var gamePlayerScores = playerScoresField.GetValue(CurrentGameInstance) as System.Collections.IDictionary;
                        if (gamePlayerScores != null)
                        {
                            foreach (System.Collections.DictionaryEntry entry in gamePlayerScores)
                            {
                                string playerId = entry.Key.ToString();
                                int score = (int)entry.Value;
                                playerScores[playerId] = score;
                            }
                        }
                    }
                }
                catch
                {
                    // Silently continue if reflection fails
                }
            }

            // Create and return memento with deep-copied state
            return new CommandMemento(commandText, playerScores, Variables);
        }

        /// <summary>
        /// Restores game state from a memento.
        /// This is called during undo operations.
        /// </summary>
        public bool RestoreFromMemento(CommandMemento memento)
        {
            if (memento == null)
                return false;

            try
            {
                // Restore player scores to context variables
                foreach (var kvp in memento.PlayerScores)
                {
                    SetVariable($"score_{kvp.Key}", kvp.Value);
                }

                // Restore scores to GameInstance if available
                if (CurrentGameInstance != null)
                {
                    var playerScoresField = CurrentGameInstance.GetType().GetField("_playerScores",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    if (playerScoresField != null)
                    {
                        var gamePlayerScores = playerScoresField.GetValue(CurrentGameInstance) as System.Collections.IDictionary;
                        if (gamePlayerScores != null)
                        {
                            foreach (var kvp in memento.PlayerScores)
                            {
                                if (gamePlayerScores.Contains(kvp.Key))
                                {
                                    gamePlayerScores[kvp.Key] = kvp.Value;

                                    // Broadcast score change to all clients
                                    var packet = NetworkPacketFactory.Instance.MakePlayerScoreSetPacket(kvp.Key, kvp.Value);
                                    packet.MessageType = (int)MessageType.GI_ServerSend_PlayerScoreSet;

                                    var componentClientsProp = CurrentGameInstance.GetType().GetProperty("ComponentClients");
                                    var componentClients = componentClientsProp?.GetValue(CurrentGameInstance) as System.Collections.IEnumerable;
                                    
                                    if (componentClients != null)
                                    {
                                        foreach (var client in componentClients)
                                        {
                                            var sendMethod = client.GetType().GetMethod("SendPacketToClient");
                                            sendMethod?.Invoke(client, new object[] { packet, MessageType.GI_ServerSend_PlayerScoreSet });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Restore other context variables
                foreach (var kvp in memento.ContextVariables)
                {
                    if (!kvp.Key.StartsWith("score_")) // Don't restore scores twice
                    {
                        Variables[kvp.Key] = kvp.Value;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}