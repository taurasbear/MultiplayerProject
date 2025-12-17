using MultiplayerProject;
using MultiplayerProject.Source.Helpers;
using MultiplayerProject.Source.Memento;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    public class CommandInterpreter
    {
        private readonly CommandParser _parser;
        private readonly GameCommandContext _context;
        private readonly CommandHistoryManager _historyManager;

        // Commands that modify state and should be saved in history
        private readonly HashSet<string> _stateChangingCommands = new HashSet<string>
        {
            "set_score"
        };

        public CommandInterpreter()
        {
            _parser = new CommandParser();
            _context = new GameCommandContext();
            _historyManager = new CommandHistoryManager(maxHistorySize: 20);
            
            // Store history manager in context so commands can access it
            _context.SetVariable("__history_manager__", _historyManager);
        }

        public CommandInterpreter(Server server) : this()
        {
            _context.Server = server;
            if (server != null)
            {
                _context.RefreshConnections();
            }
            
            // Ensure history manager is set after base constructor
            _context.SetVariable("__history_manager__", _historyManager);
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

                // Check if this is a state-changing command that needs memento
                string commandName = GetCommandName(commandText);
                bool shouldSaveState = _stateChangingCommands.Contains(commandName);

                // Create memento BEFORE executing state-changing commands
                if (shouldSaveState)
                {
                    var memento = _context.CreateMemento(commandText);
                    _historyManager.SaveMemento(memento);
                    Logger.Instance?.Info($"[MEMENTO] Saved state before: {commandText}");
                }

                var command = _parser.Parse(commandText);
                var result = command.Interpret(_context);
                
                // Log ALL commands to the command log (for history display)
                _historyManager.LogCommand(commandText);
                
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

        private string GetCommandName(string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                return string.Empty;

            string text = commandText.TrimStart('/');
            int spaceIndex = text.IndexOf(' ');
            return spaceIndex >= 0 ? text.Substring(0, spaceIndex).ToLower() : text.ToLower();
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