using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MultiplayerProject.Source.Commands;

namespace MultiplayerProject.Source
{
    public class InvalidCommand : ICommandExpression
    {
        private readonly string _errorMessage;
        public InvalidCommand(string errorMessage)
        {
            _errorMessage = errorMessage;
        }
        public string Interpret(GameCommandContext context)
        {
            return $"Error: {_errorMessage} | Type '/help' for available commands.";
        }
    }
    public class CommandParser
    {
        private readonly Dictionary<string, Func<string[], ICommandExpression>> _commandFactories;

        public CommandParser()
        {
            _commandFactories = new Dictionary<string, Func<string[], ICommandExpression>>
            {
                { "list", args => new PlayerCommand(PlayerCommand.PlayerAction.List) },
                { "info", args => new PlayerCommand(PlayerCommand.PlayerAction.Info,
                    args.Length > 0 ? args[0] : "") },
                { "stats", args => new GameCommand(GameCommand.GameAction.Stats) },
                { "set_score", args => new GameCommand(GameCommand.GameAction.SetScore,
                    args.Length > 0 ? args[0] : "",
                    args.Length > 1 ? ParseInt(args[1], 0) : 0) },
                { "help", args => new HelpCommand() }
            };
        }

        public ICommandExpression Parse(string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                return new InvalidCommand("Empty command");
            }

            if (commandText.StartsWith("/"))
            {
                commandText = commandText.Substring(1);
            }

            string[] parts = commandText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return new InvalidCommand("Empty command");
            }

            string commandName = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();

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

        public string[] GetAvailableCommands()
        {
            return _commandFactories.Keys.ToArray();
        }
        private static int ParseInt(string value, int defaultValue)
        {
            return int.TryParse(value, out int result) ? result : defaultValue;
        }
    }
}