// File: CommandInterpreter.cs
// Location: MultiplayerProject/Source/Interpreter/CommandInterpreter.cs

using MultiplayerProject;
using MultiplayerProject.Source.Helpers;
using System;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Main interpreter class that orchestrates command parsing and execution.
    /// This is the facade for the entire Interpreter pattern implementation.
    /// FULLY IMPLEMENTED with reflection-based GameInstance access.
    /// </summary>
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

        /// <summary>
        /// Set the current game instance for command context
        /// </summary>
        public void SetGameInstance(GameInstance gameInstance)
        {
            _context.CurrentGameInstance = gameInstance;
            //Logger.Instance?.Info($"[INTERPRETER] Game instance set - Commands now have full access to game state");
        }

        /// <summary>
        /// Update the list of connected players
        /// </summary>
        public void UpdateConnections(System.Collections.Generic.List<ServerConnection> connections)
        {
            _context.Connections.Clear();
            _context.Connections.AddRange(connections);
            //Logger.Instance?.Debug($"[INTERPRETER] Updated connections: {connections.Count} players");
        }

        /// <summary>
        /// Execute a command from text input with full error handling and logging
        /// </summary>
        /// <param name="commandText">The command text to parse and execute</param>
        /// <returns>Result message from command execution</returns>
        public string ExecuteCommand(string commandText)
        {
            try
            {
                // Refresh connections before command execution
                if (_context.Server != null)
                {
                    _context.RefreshConnections();
                }

                // Parse the command
                var command = _parser.Parse(commandText);

                // Log command execution start
                var debugMode = GetVariable("debug_mode", false);
                if (debugMode)
                {
                    Logger.Instance?.Debug($"[INTERPRETER] Parsing command: {commandText}");
                    Logger.Instance?.Debug($"[INTERPRETER] Command type: {command.GetType().Name}");
                }

                // Execute the command
                var result = command.Interpret(_context);

                // Log successful execution
                if (debugMode)
                {
                    Logger.Instance?.Debug($"[INTERPRETER] Command executed successfully");
                    Logger.Instance?.Debug($"[INTERPRETER] Context variables: {_context.Variables.Count}");
                }

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

        /// <summary>
        /// Get list of available commands
        /// </summary>
        public string[] GetAvailableCommands()
        {
            return _parser.GetAvailableCommands();
        }

        /// <summary>
        /// Get or set a context variable with logging
        /// </summary>
        public T GetVariable<T>(string name, T defaultValue = default(T))
        {
            var result = _context.GetVariable(name, defaultValue);
            var debugMode = _context.GetVariable("debug_mode", false);
            
            if (debugMode && !name.StartsWith("debug"))
            {
                Logger.Instance?.Debug($"[INTERPRETER] Variable '{name}' = {result}");
            }
            
            return result;
        }

        public void SetVariable<T>(string name, T value)
        {
            _context.SetVariable(name, value);
            var debugMode = _context.GetVariable("debug_mode", false);
            
            if (debugMode && !name.StartsWith("debug"))
            {
                Logger.Instance?.Debug($"[INTERPRETER] Set variable '{name}' = {value}");
            }
        }

        /// <summary>
        /// Check if server shutdown was requested via command
        /// </summary>
        public bool IsShutdownRequested()
        {
            return GetVariable("shutdown_requested", false);
        }

        /// <summary>
        /// Check if debug mode is enabled via command
        /// </summary>
        public bool IsDebugModeEnabled()
        {
            return GetVariable("debug_mode", false);
        }

        /// <summary>
        /// Get the custom enemy spawn rate if set via command
        /// </summary>
        public float? GetCustomEnemySpawnRate()
        {
            var rate = GetVariable("enemy_spawn_rate", 0f);
            return rate > 0 ? rate : (float?)null;
        }

        /// <summary>
        /// Get comprehensive interpreter statistics
        /// </summary>
        public string GetInterpreterStatus()
        {
            var stats = new System.Text.StringBuilder();
            stats.Append("=== INTERPRETER STATUS ===|");
            stats.Append($"Available Commands: {GetAvailableCommands().Length}|");
            stats.Append($"Context Variables: {_context.Variables.Count}|");
            stats.Append($"Connected Players: {_context.Connections.Count}|");
            stats.Append($"Server Instance: {(_context.Server != null ? "Available" : "Null")}|");
            stats.Append($"Game Instance: {(_context.CurrentGameInstance != null ? "Available" : "Null")}|");
            stats.Append($"Debug Mode: {IsDebugModeEnabled()}|");
            stats.Append($"Shutdown Requested: {IsShutdownRequested()}");
            
            return stats.ToString();
        }

        /// <summary>
        /// Execute a series of commands separated by semicolons
        /// </summary>
        public string ExecuteBatch(string batchCommands)
        {
            var commands = batchCommands.Split(';');
            var results = new System.Collections.Generic.List<string>();
            
            foreach (var cmd in commands)
            {
                if (!string.IsNullOrWhiteSpace(cmd))
                {
                    var result = ExecuteCommand(cmd.Trim());
                    results.Add($"[{cmd.Trim()}] {result}");
                }
            }
            
            return string.Join("|", results);
        }

        /// <summary>
        /// Clear all context variables (useful for reset)
        /// </summary>
        public void ClearContext()
        {
            var varCount = _context.Variables.Count;
            _context.Variables.Clear();
            Logger.Instance?.Info($"[INTERPRETER] Cleared {varCount} context variables");
        }

        /// <summary>
        /// Export context variables to string (for debugging/backup)
        /// </summary>
        public string ExportContext()
        {
            var export = new System.Text.StringBuilder();
            export.Append("=== CONTEXT EXPORT ===|");
            
            foreach (var kvp in _context.Variables)
            {
                export.Append($"{kvp.Key} = {kvp.Value} ({kvp.Value?.GetType().Name ?? "null"})|");
            }
            
            return export.ToString();
        }
    }
}