// File: CommandParser.cs
// Location: MultiplayerProject/Source/Interpreter/CommandParser.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MultiplayerProject.Source.Commands;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Parser class that converts text input into command expressions.
    /// Implements the parsing logic for the Interpreter pattern.
    /// </summary>
    public class CommandParser
    {
        private readonly Dictionary<string, Func<string[], ICommandExpression>> _commandFactories;

        public CommandParser()
        {
            _commandFactories = new Dictionary<string, Func<string[], ICommandExpression>>
            {
                // Player management commands (3)
                { "kick", args => new PlayerCommand(PlayerCommand.PlayerAction.Kick, 
                    args.Length > 0 ? args[0] : "") },
                { "list", args => new PlayerCommand(PlayerCommand.PlayerAction.List) },
                { "info", args => new PlayerCommand(PlayerCommand.PlayerAction.Info,
                    args.Length > 0 ? args[0] : "") },
                
                // Game management commands (4)
                { "stats", args => new GameCommand(GameCommand.GameAction.Stats) },
                { "set_score", args => new GameCommand(GameCommand.GameAction.SetScore,
                    args.Length > 0 ? args[0] : "",
                    args.Length > 1 ? ParseInt(args[1], 0) : 0) },
                { "restart", args => new GameCommand(GameCommand.GameAction.Restart) },
                { "trigger_stats", args => new GameCommand(GameCommand.GameAction.TriggerStats) },
                
                // Enemy management commands (3)
                { "spawn", args => new EnemyCommand(EnemyCommand.EnemyAction.Spawn,
                    args.Length > 0 ? ParseEnemyType(args[0]) : EnemyType.Bird,
                    args.Length > 1 ? ParseFloat(args[1], 400f) : 400f,
                    args.Length > 2 ? ParseFloat(args[2], 300f) : 300f) },
                { "clear", args => new EnemyCommand(EnemyCommand.EnemyAction.Clear) },
                { "spawn_rate", args => new EnemyCommand(EnemyCommand.EnemyAction.SetSpawnRate,
                    args.Length > 0 ? ParseFloat(args[0], 1.0f) : 1.0f) },
                
                // Server commands (1)
                { "shutdown", args => new ShutdownServerCommand() },
                
                // Help command (1)
                { "help", args => new HelpCommand() }
                
                // Total: 12 core commands
            };
        }

        /// <summary>
        /// Parse a text command into an executable command expression
        /// </summary>
        public ICommandExpression Parse(string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                return new InvalidCommand("Empty command");
            }

            // Remove leading slash if present
            if (commandText.StartsWith("/"))
            {
                commandText = commandText.Substring(1);
            }

            // Split command into parts
            string[] parts = commandText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return new InvalidCommand("Empty command");
            }

            string commandName = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();

            // Look up command factory
            if (_commandFactories.ContainsKey(commandName))
            {
                try
                {
                    return _commandFactories[commandName](args);
                }
                catch (Exception ex)
                {
                    return new InvalidCommand($"Error parsing command '{commandName}': {ex.Message}");
                }
            }
            else
            {
                return new InvalidCommand($"Unknown command: {commandName}. Type '/help' for available commands.");
            }
        }

        /// <summary>
        /// Get list of all available commands
        /// </summary>
        public string[] GetAvailableCommands()
        {
            return _commandFactories.Keys.ToArray();
        }

        // Helper parsing methods
        private static int ParseInt(string value, int defaultValue)
        {
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        private static float ParseFloat(string value, float defaultValue)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result) ? result : defaultValue;
        }

        private static bool ParseBool(string value, bool defaultValue)
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;
            
            value = value.ToLower();
            if (value == "true" || value == "on" || value == "1" || value == "yes")
                return true;
            if (value == "false" || value == "off" || value == "0" || value == "no")
                return false;
                
            return defaultValue;
        }

        private static EnemyType ParseEnemyType(string value)
        {
            if (Enum.TryParse<EnemyType>(value, true, out EnemyType result))
            {
                return result;
            }
            return EnemyType.Bird; // Default
        }
    }
}