using MultiplayerProject;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
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
    }
}