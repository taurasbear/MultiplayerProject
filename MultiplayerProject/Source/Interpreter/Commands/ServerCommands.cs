// File: ServerCommands.cs
// Location: MultiplayerProject/Source/Interpreter/Commands/ServerCommands.cs

using System.Text;
using System.Reflection;
using System.Linq;

namespace MultiplayerProject.Source.Commands
{
    /// <summary>
    /// Unified command for all server-related operations
    /// </summary>
    public class ServerCommand : ICommandExpression
    {
        public enum ServerAction
        {
            Shutdown,
            Help,
            Invalid
        }

        private readonly ServerAction _action;
        private readonly string _errorMessage;
        private readonly string _originalCommand;

        // Constructor for shutdown and help
        public ServerCommand(ServerAction action)
        {
            _action = action;
        }

        // Constructor for invalid commands
        public ServerCommand(ServerAction action, string errorMessage, string originalCommand = "")
        {
            _action = action;
            _errorMessage = errorMessage;
            _originalCommand = originalCommand;
        }

        public string Interpret(GameCommandContext context)
        {
            switch (_action)
            {
                case ServerAction.Shutdown:
                    return ShutdownServer(context);
                case ServerAction.Help:
                    return ShowHelp(context);
                case ServerAction.Invalid:
                    return HandleInvalidCommand(context);
                default:
                    return "Error: Unknown server action.";
            }
        }

        private string ShutdownServer(GameCommandContext context)
        {
            if (context.Server == null)
                return "Error: No server instance available.";

            context.SetVariable("shutdown_requested", true);
            context.SetVariable("shutdown_time", System.DateTime.UtcNow);
            
            return "Server shutdown initiated.|" +
                   "All players will be disconnected.|" +
                   "Game instances will be terminated.|" +
                   "Server will stop accepting new connections.|" +
                   $"Shutdown requested at: {System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
        }

        private string ShowHelp(GameCommandContext context)
        {
            var sb = new StringBuilder();
            sb.Append("=== CORE COMMAND REFERENCE ===|");
            sb.Append("|Player Management:|");
            sb.Append("  /kick <player>              - Remove player from server|");
            sb.Append("  /list                       - List all connected players|");
            sb.Append("  /info <player>              - Show player information|");
            sb.Append("|");
            sb.Append("Game Management:|");
            sb.Append("  /stats                      - Show game statistics|");
            sb.Append("  /set_score <player> <score> - Set player score|");
            sb.Append("  /restart                    - Restart current game|");
            sb.Append("  /trigger_stats              - Manual visitor stats trigger|");
            sb.Append("|");
            sb.Append("Enemy Management:|");
            sb.Append("  /spawn <type> <x> <y>       - Spawn enemy (bird/blackbird/mine)|");
            sb.Append("  /clear                      - Remove all enemies|");
            sb.Append("  /spawn_rate <seconds>       - Set enemy spawn interval|");
            sb.Append("|");
            sb.Append("Server Control:|");
            sb.Append("  /shutdown                   - Shutdown server|");
            sb.Append("  /help                       - Show this help|");
            sb.Append("|");
            sb.Append("Controls: ~ or / (toggle), Enter (execute), Escape (cancel)|");
            sb.Append("Example: /kick PlayerOne");
            
            return sb.ToString();
        }

        private string HandleInvalidCommand(GameCommandContext context)
        {
            var result = $"Error: {_errorMessage}";
            
            if (!string.IsNullOrEmpty(_originalCommand))
            {
                var suggestions = GetCommandSuggestions(_originalCommand);
                if (suggestions.Length > 0)
                {
                    result += $"|Did you mean: {string.Join(", ", suggestions)}?";
                }
            }
            
            result += "|Type '/help' for available commands.";
            return result;
        }

        private string[] GetCommandSuggestions(string command)
        {
            var allCommands = new[]
            {
                "help", "list", "stats", "kick", "spawn", "clear",
                "restart", "shutdown", "info", "spawn_rate"
            };
            
            var suggestions = new System.Collections.Generic.List<string>();
            
            foreach (var cmd in allCommands)
            {
                if (cmd.StartsWith(command) || 
                    System.Math.Abs(cmd.Length - command.Length) <= 2)
                {
                    suggestions.Add(cmd);
                }
            }
            
            return suggestions.Take(3).ToArray();
        }
    }

    // Alias classes for backward compatibility
    public class ShutdownServerCommand : ICommandExpression
    {
        public string Interpret(GameCommandContext context)
        {
            return new ServerCommand(ServerCommand.ServerAction.Shutdown).Interpret(context);
        }
    }

    public class HelpCommand : ICommandExpression
    {
        public string Interpret(GameCommandContext context)
        {
            return new ServerCommand(ServerCommand.ServerAction.Help).Interpret(context);
        }
    }

    public class InvalidCommand : ICommandExpression
    {
        private readonly string _errorMessage;
        private readonly string _originalCommand;

        public InvalidCommand(string errorMessage, string originalCommand = "")
        {
            _errorMessage = errorMessage;
            _originalCommand = originalCommand;
        }

        public string Interpret(GameCommandContext context)
        {
            return new ServerCommand(ServerCommand.ServerAction.Invalid, _errorMessage, _originalCommand).Interpret(context);
        }
    }
}