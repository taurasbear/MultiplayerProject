using System.Text;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace MultiplayerProject.Source.Commands
{
    public class GameCommand : ICommandExpression
    {
        public enum GameAction
        {
            Stats,
            SetScore
        }

        private readonly GameAction _action;
        private readonly string _playerName;
        private readonly int _score;

        public GameCommand(GameAction action, string playerName, int score)
        {
            _action = action;
            _playerName = playerName;
            _score = score;
        }

        public GameCommand(GameAction action)
        {
            _action = action;
        }

        public string Interpret(GameCommandContext context)
        {
            switch (_action)
            {
                case GameAction.Stats:
                    return GetGameStats(context);
                case GameAction.SetScore:
                    return SetPlayerScore(context);
                default:
                    return "Error: Unknown game action.";
            }
        }

        private string GetGameStats(GameCommandContext context)
        {
            var result = new StringBuilder();

            if (context.CurrentGameInstance != null)
            {
                
                var playerScores = GetPrivateField(context.CurrentGameInstance, "_playerScores") as System.Collections.IDictionary;
                var playerNames = GetPrivateField(context.CurrentGameInstance, "_playerNames") as System.Collections.IDictionary;
                var scoreVisitor = GetPrivateField(context.CurrentGameInstance, "_scoreVisitor");
                
                if (playerScores != null && scoreVisitor != null)
                {
                    result.Append($"Active Players in Game: {playerScores.Count}|");
                    result.Append("|=== PLAYER SCORES ===|");
                    
                    // Reset the visitor and populate it with current scores
                    var resetMethod = scoreVisitor.GetType().GetMethod("Reset");
                    resetMethod?.Invoke(scoreVisitor, null);
                    
                    var addPlayerScoreMethod = scoreVisitor.GetType().GetMethod("AddPlayerScore");
                    
                    foreach (System.Collections.DictionaryEntry entry in playerScores)
                    {
                        string playerName = playerNames?[entry.Key]?.ToString() ?? entry.Key.ToString();
                        int score = (int)entry.Value;
                        result.Append($"  {playerName}: {score} points|");
                        
                        // Add score to visitor for statistical analysis
                        addPlayerScoreMethod?.Invoke(scoreVisitor, new object[] { entry.Key.ToString(), score });
                    }
                    
                    // Use visitor's own logging method and get result
                    var logScoreMethod = scoreVisitor.GetType().GetMethod("LogScoreReport");
                    string scoreResult = (string)logScoreMethod?.Invoke(scoreVisitor, null);
                    if (!string.IsNullOrEmpty(scoreResult))
                    {
                        result.Append($"{scoreResult}|");
                    }
                }

                var activeVisitor = GetPrivateField(context.CurrentGameInstance, "_activeStatsVisitor");
                var lifetimeVisitor = GetPrivateField(context.CurrentGameInstance, "_lifetimeStatsVisitor");
                
                if (activeVisitor != null)
                {
                    result.Append("|=== LAST 5 SEC STATISTICS ===|");
                    var logCurrentMethod = activeVisitor.GetType().GetMethod("LogCurrentStatus");
                    string activeResult = (string)logCurrentMethod?.Invoke(activeVisitor, null);
                    if (!string.IsNullOrEmpty(activeResult))
                    {
                        result.Append($"{activeResult}|");
                    }
                }

                if (lifetimeVisitor != null)
                {
                    result.Append("|=== LIFETIME STATISTICS ===|");
                    var logLifetimeMethod = lifetimeVisitor.GetType().GetMethod("LogLifetimeReport");
                    string lifetimeResult = (string)logLifetimeMethod?.Invoke(lifetimeVisitor, null);
                    if (!string.IsNullOrEmpty(lifetimeResult))
                    {
                        result.Append($"{lifetimeResult}|");
                    }
                }
            }
            else
            {
                result.Append("|No active game instance. Statistics not available.|");
            }
;
            
            return result.ToString();
        }

        private string SetPlayerScore(GameCommandContext context)
        {
            if (string.IsNullOrWhiteSpace(_playerName))
                return "Error: Player name required. Usage: /set_score <playername> <score>";

            if (context.CurrentGameInstance == null)
                return "Error: No active game instance. Cannot set player score.";

            var player = context.FindPlayerByName(_playerName);
            if (player == null)
                return $"Error: Player '{_playerName}' not found. Use /list to see connected players.";

            try
            {
                var playerScores = GetPrivateField(context.CurrentGameInstance, "_playerScores") as System.Collections.IDictionary;
                if (playerScores != null && playerScores.Contains(player.ID))
                {
                    playerScores[player.ID] = _score;
                    context.SetPlayerScore(player.ID, _score);

                    // Broadcast score change to all clients
                    var gameInstance = context.CurrentGameInstance;
                    var componentClientsProp = gameInstance.GetType().GetProperty("ComponentClients");
                    var componentClients = componentClientsProp?.GetValue(gameInstance) as System.Collections.IEnumerable;
                    var packet = MultiplayerProject.Source.NetworkPacketFactory.Instance.MakePlayerScoreSetPacket(player.ID, _score);
                    packet.MessageType = (int)MultiplayerProject.Source.MessageType.GI_ServerSend_PlayerScoreSet;
                    if (componentClients != null)
                    {
                        foreach (var client in componentClients)
                        {
                            var sendMethod = client.GetType().GetMethod("SendPacketToClient");
                            sendMethod?.Invoke(client, new object[] { packet, MultiplayerProject.Source.MessageType.GI_ServerSend_PlayerScoreSet });
                        }
                    }

                    return $"Player '{_playerName}' score set to {_score}. ";
                }
                else
                {
                    context.SetPlayerScore(player.ID, _score);
                    return $"Player '{_playerName}' score set to {_score}. (player may not be in active game)";
                }
            }
            catch (System.Exception ex)
            {
                context.SetPlayerScore(player.ID, _score);
                return $"Player '{_playerName}' score set to {_score}. (GameInstance update failed: {ex.Message})";
            }
        }

        private object GetPrivateField(object obj, string fieldName)
        {
            if (obj == null) return null;
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(obj);
        }

    }
}