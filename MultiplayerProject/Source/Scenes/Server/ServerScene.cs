using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiplayerProject;

namespace MultiplayerProject.Source
{
    public class ServerScene
    {
        private SpriteFont _font;
        private GraphicsDevice _device;

        public int Width { get; set; }
        public int Height { get; set; }

        private const float TEXT_SCALE = 0.5f;
        private const float START_Y_POS = 50f;

        // Title
        private Vector2 _titlePosition;
        private const string _titleText = "SERVER LOG";

        private List<string> logLines;
        private string _path;
        private float yGap;

        // For scrolling
        private int scrollOffset = 0;

        // Command interpreter for Interpreter pattern - FULLY IMPLEMENTED
        private CommandInterpreter _fullCommandInterpreter;
        private StringBuilder _currentCommand;
        private bool _commandMode = false;
        private KeyboardState _previousKeyboardState;

        // Server reference for command execution
        private Server _server;

        public ServerScene(int width, int height)
        {
            Width = width;
            Height = height;

            _path = Assembly.GetExecutingAssembly().GetName().Name + ".log";

            bool b;
            logLines = Logger.Instance.ReadLastLines(0, 60, out b);

            Logger.OnNewLogItem += Logger_OnNewLogItem;

            // Initialize command interpreter - FULL IMPLEMENTATION
            _fullCommandInterpreter = new CommandInterpreter();
            _currentCommand = new StringBuilder();
            _previousKeyboardState = Keyboard.GetState();
            
        }

        /// <summary>
        /// Set the server instance for command execution
        /// </summary>
        public void SetServer(Server server)
        {
            _server = server;
            // Initialize command interpreter with server instance
            _fullCommandInterpreter = new CommandInterpreter(server);
        }

        /// <summary>
        /// Update the command interpreter with current game state
        /// </summary>
        public void UpdateCommandContext(GameInstance gameInstance, List<ServerConnection> connections)
        {
            if (_fullCommandInterpreter != null)
            {
                _fullCommandInterpreter.SetGameInstance(gameInstance);
                _fullCommandInterpreter.UpdateConnections(connections ?? new List<ServerConnection>());
            }
        }

        /// <summary>
        /// Update the command interpreter with server connections only
        /// </summary>
        public void UpdateServerConnections()
        {
            if (_fullCommandInterpreter != null && _server != null)
            {
                _fullCommandInterpreter.UpdateConnections(_server.ComponentClients);
            }
        }

        private void Logger_OnNewLogItem(string str)
        {
            logLines.Insert(0, str);
            // Optionally, keep scroll at bottom if already at bottom
            if (scrollOffset == 0)
            {
                // No action needed, stays at bottom
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw title
            spriteBatch.DrawString(_font, _titleText, _titlePosition, Color.White);

            // Calculate how many lines fit in the window
            int maxVisibleLines = (int)((Height - START_Y_POS) / yGap);
            if (maxVisibleLines < 1) maxVisibleLines = 1;

            // Reserve space for command input if in command mode
            if (_commandMode)
            {
                maxVisibleLines -= 2; // Reserve 2 lines for command input
            }

            // Clamp scrollOffset
            int maxOffset = Math.Max(0, logLines.Count - maxVisibleLines);
            if (scrollOffset > maxOffset) scrollOffset = maxOffset;
            if (scrollOffset < 0) scrollOffset = 0;

            int count = 0;
            for (int i = scrollOffset; i < logLines.Count && count < maxVisibleLines; i++)
            {
                if (logLines[i] != null && !string.IsNullOrEmpty(logLines[i]))
                {
                    try
                    {
                        spriteBatch.DrawString(_font, logLines[i], new Vector2(0, START_Y_POS + (count * yGap)), Color.Black, 0, Vector2.Zero, TEXT_SCALE, SpriteEffects.None, 0);
                        count++;
                    }
                    catch { /* This is bad */ }
                }
            }

            // Draw command input if in command mode
            if (_commandMode)
            {
                float commandY = Height - (yGap * 2);
                spriteBatch.DrawString(_font, "Command: /" + _currentCommand.ToString() + "_", 
                    new Vector2(10, commandY), Color.Yellow, 0, Vector2.Zero, TEXT_SCALE, SpriteEffects.None, 0);
                spriteBatch.DrawString(_font, "Press Enter to execute, Escape to cancel, ~ or / to toggle", 
                    new Vector2(10, commandY + yGap), Color.Gray, 0, Vector2.Zero, TEXT_SCALE, SpriteEffects.None, 0);
            }
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Font");
            yGap = _font.MeasureString("T").Y * TEXT_SCALE;
            _device = graphicsDevice;

            _titlePosition = new Vector2(Application.WINDOW_WIDTH / 2, 0);
            _titlePosition.X -= (_font.MeasureString(_titleText).X / 2);
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            var currentKeyboardState = inputInfo.CurrentKeyboardState;

            // Check for command mode toggle keys
            bool tildePressed = currentKeyboardState.IsKeyDown(Keys.OemTilde) && !_previousKeyboardState.IsKeyDown(Keys.OemTilde);
            bool slashPressed = currentKeyboardState.IsKeyDown(Keys.OemQuestion) && !_previousKeyboardState.IsKeyDown(Keys.OemQuestion);

            // Toggle command mode with tilde (~) or forward slash (/) key
            if (tildePressed || slashPressed)
            {
                _commandMode = !_commandMode;
                if (!_commandMode)
                {
                    _currentCommand.Clear();
                }
                Logger.Instance?.Info($"[COMMAND] Mode {(_commandMode ? "ENABLED" : "DISABLED")}");
                
                // Don't process any other input this frame to prevent the activation key from being added
                _previousKeyboardState = currentKeyboardState;
                return;
            }

            if (_commandMode)
            {
                ProcessCommandInput(currentKeyboardState);
            }
            else
            {
                ProcessNormalInput(inputInfo);
            }

            _previousKeyboardState = currentKeyboardState;
        }

        private void ProcessCommandInput(KeyboardState currentKeyboardState)
        {
            // Handle command input
            if (currentKeyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter))
            {
                // Execute command
                ExecuteCurrentCommand();
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
            {
                // Cancel command
                _commandMode = false;
                _currentCommand.Clear();
                Logger.Instance?.Info("[COMMAND] Cancelled");
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Back) && !_previousKeyboardState.IsKeyDown(Keys.Back))
            {
                // Backspace
                if (_currentCommand.Length > 0)
                {
                    _currentCommand.Remove(_currentCommand.Length - 1, 1);
                }
            }
            else
            {
                // Add typed characters
                var pressedKeys = currentKeyboardState.GetPressedKeys();
                foreach (var key in pressedKeys)
                {
                    if (!_previousKeyboardState.IsKeyDown(key))
                    {
                        char? character = KeyToChar(key, currentKeyboardState.IsKeyDown(Keys.LeftShift) || currentKeyboardState.IsKeyDown(Keys.RightShift));
                        if (character.HasValue && _currentCommand.Length < 100) // Limit command length
                        {
                            _currentCommand.Append(character.Value);
                        }
                    }
                }
            }
        }

        private void ProcessNormalInput(InputInformation inputInfo)
        {
            // Mouse wheel scroll: positive delta = up, negative = down
            int wheelDelta = inputInfo.CurrentMouseState.ScrollWheelValue - inputInfo.PreviousMouseState.ScrollWheelValue;
            if (wheelDelta != 0)
            {
                // Typical mouse wheel delta is 120 per notch
                int linesToScroll = wheelDelta / 120;
                // Invert so wheel up scrolls up (older logs), wheel down scrolls down (newer logs)
                scrollOffset -= linesToScroll;
            }
        }

        private void ExecuteCurrentCommand()
        {
            if (_currentCommand.Length == 0)
            {
                _commandMode = false;
                return;
            }

            string commandText = _currentCommand.ToString();
            Logger.Instance?.Info($"[COMMAND] {commandText}");

            try
            {
                string result = _fullCommandInterpreter.ExecuteCommand("/" + commandText);
                
                Logger.Instance?.Info("[RESULT] ----------------------------------------");
                
                var lines = result.Split('|');
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        Logger.Instance?.Info($"[RESULT] {line.Trim()}");
                    }
                }
                
                Logger.Instance?.Info("[RESULT] ----------------------------------------");

                if (_fullCommandInterpreter.IsShutdownRequested())
                {
                    Logger.Instance?.Info("[SHUTDOWN] Requested via command");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance?.Error($"[ERROR] {ex.Message}");
            }

            _currentCommand.Clear();
            _commandMode = false;
        }

        private char? KeyToChar(Keys key, bool shift)
        {
            // Basic character mapping for common keys
            switch (key)
            {
                case Keys.A: return shift ? 'A' : 'a';
                case Keys.B: return shift ? 'B' : 'b';
                case Keys.C: return shift ? 'C' : 'c';
                case Keys.D: return shift ? 'D' : 'd';
                case Keys.E: return shift ? 'E' : 'e';
                case Keys.F: return shift ? 'F' : 'f';
                case Keys.G: return shift ? 'G' : 'g';
                case Keys.H: return shift ? 'H' : 'h';
                case Keys.I: return shift ? 'I' : 'i';
                case Keys.J: return shift ? 'J' : 'j';
                case Keys.K: return shift ? 'K' : 'k';
                case Keys.L: return shift ? 'L' : 'l';
                case Keys.M: return shift ? 'M' : 'm';
                case Keys.N: return shift ? 'N' : 'n';
                case Keys.O: return shift ? 'O' : 'o';
                case Keys.P: return shift ? 'P' : 'p';
                case Keys.Q: return shift ? 'Q' : 'q';
                case Keys.R: return shift ? 'R' : 'r';
                case Keys.S: return shift ? 'S' : 's';
                case Keys.T: return shift ? 'T' : 't';
                case Keys.U: return shift ? 'U' : 'u';
                case Keys.V: return shift ? 'V' : 'v';
                case Keys.W: return shift ? 'W' : 'w';
                case Keys.X: return shift ? 'X' : 'x';
                case Keys.Y: return shift ? 'Y' : 'y';
                case Keys.Z: return shift ? 'Z' : 'z';
                
                case Keys.D0: return shift ? ')' : '0';
                case Keys.D1: return shift ? '!' : '1';
                case Keys.D2: return shift ? '@' : '2';
                case Keys.D3: return shift ? '#' : '3';
                case Keys.D4: return shift ? '$' : '4';
                case Keys.D5: return shift ? '%' : '5';
                case Keys.D6: return shift ? '^' : '6';
                case Keys.D7: return shift ? '&' : '7';
                case Keys.D8: return shift ? '*' : '8';
                case Keys.D9: return shift ? '(' : '9';
                
                case Keys.Space: return ' ';
                case Keys.OemMinus: return shift ? '_' : '-';
                case Keys.OemPeriod: return shift ? '>' : '.';
                case Keys.OemQuestion: return shift ? '?' : '/';
                case Keys.OemTilde: return shift ? '~' : '`';
                
                default: return null;
            }
        }

        public void Update(GameTime gameTime)
        {
            // Update command interpreter with latest server state every frame
            UpdateServerConnections();
            
            // Also update with active game instance if available
            if (_server != null)
            {
                var activeGameInstance = _server.GetActiveGameInstance();
                if (activeGameInstance != null && _fullCommandInterpreter != null)
                {
                    _fullCommandInterpreter.SetGameInstance(activeGameInstance);
                }
            }
        }
    }
}
