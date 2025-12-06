using MultiplayerProject;
using MultiplayerProject.Source.Helpers;
using System;

namespace MultiplayerProject.Source
{
    public class CommandInterpreter
    {
        private readonly CommandParser _parser;
        private readonly GameCommandContext _context;

        public CommandInterpreter()
        {
            _parser = new CommandParser();
            _context = new GameCommandContext();
        }

        public CommandInterpreter(Server server) : this()
        {
            _context.Server = server;
            if (server != null)
            {
                _context.RefreshConnections();
            }
        }

        public void SetGameInstance(GameInstance gameInstance)
        {
            _context.CurrentGameInstance = gameInstance;
        }

        public void UpdateConnections(System.Collections.Generic.List<ServerConnection> connections)
        {
            _context.Connections.Clear();
            _context.Connections.AddRange(connections);
        }

        public string ExecuteCommand(string commandText)
        {
            try
            {
                if (_context.Server != null)
                {
                    _context.RefreshConnections();
                }

                var command = _parser.Parse(commandText);
                var result = command.Interpret(_context);
                return result;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Command execution failed: {ex.Message}";
                Logger.Instance?.Error($"[INTERPRETER] {errorMessage}");
                Logger.Instance?.Debug($"[INTERPRETER] Stack trace: {ex.StackTrace}");
                return errorMessage;
            }
        }

        public string[] GetAvailableCommands()
        {
            return _parser.GetAvailableCommands();
        }

        public T GetVariable<T>(string name, T defaultValue = default(T))
        {
            return _context.GetVariable(name, defaultValue);
        }
    }
}