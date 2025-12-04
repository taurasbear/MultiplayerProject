// File: PlayerCommands.cs
// Location: MultiplayerProject/Source/Interpreter/Commands/PlayerCommands.cs

using System.Linq;
using System.Text;
using System.Reflection;

namespace MultiplayerProject.Source.Commands
{
    /// <summary>
    /// Unified command for all player-related operations
    /// </summary>
    public class PlayerCommand : ICommandExpression
    {
        public enum PlayerAction
        {
            Kick,
            List,
            Info
        }

        private readonly PlayerAction _action;
        private readonly string _playerName;

        // Constructor for actions that need a player name
        public PlayerCommand(PlayerAction action, string playerName)
        {
            _action = action;
            _playerName = playerName;
        }

        // Constructor for actions that don't need parameters
        public PlayerCommand(PlayerAction action)
        {
            _action = action;
        }

        public string Interpret(GameCommandContext context)
        {
            switch (_action)
            {
                case PlayerAction.Kick:
                    return KickPlayer(context);
                case PlayerAction.List:
                    return ListPlayers(context);
                case PlayerAction.Info:
                    return GetPlayerInfo(context);
                default:
                    return "Error: Unknown player action.";
            }
        }

        private string KickPlayer(GameCommandContext context)
        {
            if (string.IsNullOrWhiteSpace(_playerName))
                return "Error: Player name required. Usage: /kick <playername>";

            context.RefreshConnections();
            var player = context.FindPlayerByName(_playerName);
            if (player == null)
                return $"Error: Player '{_playerName}' not found. Use /list to see connected players.";

            try
            {
                if (context.CurrentGameInstance != null)
                    context.CurrentGameInstance.RemoveClient(player);

                if (context.Server != null)
                    context.Server.RemoveClient(player);

                player.StopAll();
                
                return $"Player '{_playerName}' (ID: {player.ID}) has been kicked from the server.";
            }
            catch (System.Exception ex)
            {
                return $"Error kicking player '{_playerName}': {ex.Message}";
            }
        }

        private string ListPlayers(GameCommandContext context)
        {
            context.RefreshConnections();
            
            if (context.Connections.Count == 0)
                return "No players currently connected to the server.";

            var sb = new StringBuilder();
            sb.Append($"=== CONNECTED PLAYERS ({context.Connections.Count}) ===|");
            
            for (int i = 0; i < context.Connections.Count; i++)
            {
                var connection = context.Connections[i];
                var inGame = context.CurrentGameInstance?.ComponentClients.Contains(connection) ?? false;
                var score = context.GetPlayerScore(connection.ID);
                
                sb.Append($"{i + 1}. {connection.Name} (ID: {connection.ID.Substring(0, 8)}...)|");
                sb.Append($"   Status: {(inGame ? "In Game" : "In Lobby")}|");
                if (inGame && score > 0)
                {
                    sb.Append($"   Score: {score}|");
                }
            }

            if (context.CurrentGameInstance != null)
            {
                var gamePlayerCount = context.CurrentGameInstance.ComponentClients.Count;
                sb.Append($"|Active Game Instance: {gamePlayerCount} players|");
            }

            return sb.ToString().TrimEnd('|');
        }

        private string GetPlayerInfo(GameCommandContext context)
        {
            if (string.IsNullOrWhiteSpace(_playerName))
                return "Error: Player name required. Usage: /info <playername>";

            context.RefreshConnections();
            var player = context.FindPlayerByName(_playerName);
            if (player == null)
                return $"Error: Player '{_playerName}' not found. Use /list to see connected players.";

            var sb = new StringBuilder();
            sb.Append($"=== PLAYER INFORMATION: {player.Name} ===|");
            sb.Append($"Player ID: {player.ID}|");
            sb.Append($"Connected: Yes|");
            sb.Append($"Socket Status: {(player.ClientSocket?.Connected ?? false ? "Connected" : "Disconnected")}|");
            
            var inGame = context.CurrentGameInstance?.ComponentClients.Contains(player) ?? false;
            sb.Append($"In Game: {(inGame ? "Yes" : "No")}|");
            
            if (inGame && context.CurrentGameInstance != null)
            {
                try
                {
                    var playerScores = GetPrivateField(context.CurrentGameInstance, "_playerScores") as System.Collections.IDictionary;
                    var playerElements = GetPrivateField(context.CurrentGameInstance, "_playerElements") as System.Collections.IDictionary;
                    
                    if (playerScores?.Contains(player.ID) == true)
                        sb.Append($"Current Score: {playerScores[player.ID]}|");
                    
                    if (playerElements?.Contains(player.ID) == true)
                        sb.Append($"Element Type: {playerElements[player.ID]}|");
                }
                catch (System.Exception ex)
                {
                    sb.Append($"Game Info Error: {ex.Message}|");
                }
            }

            var storedScore = context.GetPlayerScore(player.ID);
            if (storedScore > 0)
                sb.Append($"Stored Score (Context): {storedScore}|");

            return sb.ToString().TrimEnd('|');
        }

        private object GetPrivateField(object obj, string fieldName)
        {
            if (obj == null) return null;
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(obj);
        }
    }
}