// File: GameCommands.cs
// Location: MultiplayerProject/Source/Interpreter/Commands/GameCommands.cs

using System.Text;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace MultiplayerProject.Source.Commands
{
    /// <summary>
    /// Unified command for all game-related operations
    /// </summary>
    public class GameCommand : ICommandExpression
    {
        public enum GameAction
        {
            Stats,
            SetScore,
            Restart,
            TriggerStats
        }

        private readonly GameAction _action;
        private readonly string _playerName;
        private readonly int _score;

        // Constructor for actions with player and score
        public GameCommand(GameAction action, string playerName, int score)
        {
            _action = action;
            _playerName = playerName;
            _score = score;
        }

        // Constructor for actions with no parameters
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
                case GameAction.Restart:
                    return RestartGame(context);
                case GameAction.TriggerStats:
                    return TriggerStats(context);
                default:
                    return "Error: Unknown game action.";
            }
        }

        private string GetGameStats(GameCommandContext context)
        {
            var result = new StringBuilder();

            // Game instance information
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
                    return $"Player '{_playerName}' score set to {_score}. (GameInstance updated)";
                }
                else
                {
                    context.SetPlayerScore(player.ID, _score);
                    return $"Player '{_playerName}' score set to {_score}. (Context only - player may not be in active game)";
                }
            }
            catch (System.Exception ex)
            {
                context.SetPlayerScore(player.ID, _score);
                return $"Player '{_playerName}' score set to {_score}. (Context only - GameInstance update failed: {ex.Message})";
            }
        }

        private string RestartGame(GameCommandContext context)
        {
            if (context.CurrentGameInstance == null)
                return "No active game instance to restart.";

            try
            {
                var lifetimeVisitor = GetPrivateField(context.CurrentGameInstance, "_lifetimeStatsVisitor");
                if (lifetimeVisitor != null)
                {
                    SetPrivateField(lifetimeVisitor, "_totalLasersFired", 0);
                    SetPrivateField(lifetimeVisitor, "_totalExplosions", 0);
                    SetPrivateField(lifetimeVisitor, "_totalEnemiesSpawned", 0);
                }

                var playerScores = GetPrivateField(context.CurrentGameInstance, "_playerScores") as System.Collections.IDictionary;
                if (playerScores != null)
                {
                    var keys = new object[playerScores.Count];
                    playerScores.Keys.CopyTo(keys, 0);
                    foreach (var key in keys)
                    {
                        playerScores[key] = 0;
                    }
                }

                return "Game restart initiated:|" +
                       "- Player scores reset to 0|" +
                       "- Lifetime statistics reset|" +
                       "- All players will be notified via normal game mechanics";
            }
            catch (System.Exception ex)
            {
                return $"Game restart partially completed. Error: {ex.Message}";
            }
        }

        private string TriggerStats(GameCommandContext context)
        {
            if (context.CurrentGameInstance == null)
                return "No active game instance. Cannot trigger statistics.";

            try
            {
                var activeVisitor = GetPrivateField(context.CurrentGameInstance, "_activeStatsVisitor");
                var lifetimeVisitor = GetPrivateField(context.CurrentGameInstance, "_lifetimeStatsVisitor");
                var scoreVisitor = GetPrivateField(context.CurrentGameInstance, "_scoreVisitor");

                var result = "=== MANUALLY TRIGGERED VISITOR STATISTICS ===|";
                
                if (activeVisitor != null)
                {
                    var logCurrentMethod = activeVisitor.GetType().GetMethod("LogCurrentStatus");
                    var logRecentMethod = activeVisitor.GetType().GetMethod("LogRecentActivity");
                    
                    string activeResult = (string)logCurrentMethod?.Invoke(activeVisitor, null);
                    logRecentMethod?.Invoke(activeVisitor, null);
                    
                    result += "Active Objects Visitor: Logged current and recent statistics|";
                }

                if (lifetimeVisitor != null)
                {
                    var logLifetimeMethod = lifetimeVisitor.GetType().GetMethod("LogLifetimeReport");
                    string lifetimeResult = (string)logLifetimeMethod?.Invoke(lifetimeVisitor, null);
                    
                    result += "Lifetime Statistics Visitor: Logged lifetime report|";
                }

                if (scoreVisitor != null)
                {
                    var logScoreMethod = scoreVisitor.GetType().GetMethod("LogScoreReport");
                    string scoreResult = (string)logScoreMethod?.Invoke(scoreVisitor, null);
                    
                    result += "Player Score Visitor: Logged score report|";
                }

                result += "|Check server logs for detailed visitor pattern output.";
                return result;
            }
            catch (System.Exception ex)
            {
                return $"Error triggering visitor statistics: {ex.Message}";
            }
        }

        private object GetPrivateField(object obj, string fieldName)
        {
            if (obj == null) return null;
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(obj);
        }

        private object GetPublicProperty(object obj, string propertyName)
        {
            if (obj == null) return null;
            var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return property?.GetValue(obj);
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            if (obj == null) return;
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}