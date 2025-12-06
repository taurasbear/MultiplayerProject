using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame_Textbox;

namespace MultiplayerProject.Source
{
    public class MainMenu
    {
        public static event StringDelegate OnServerStartRequested;
        public static event StringDelegate OnClientStartRequested;

        private TextBox _textbox;
        private Rectangle _viewPort;
        private string _textboxText = "Player";

        private SpriteFont _font;

        private Vector2 _titlePosition;
        private const string _titleText = "ENTER YOUR PLAYER NAME";

        // Connect button
        private Vector2 _connectBtnPosition;
        private Rectangle _connectBtnRect;
        private Color _connectBtnColour;
        private string _connectBtnText = "CONNECT";

        // Start server button
        private Vector2 _serverBtnPosition;
        private Rectangle _serverBtnRect;
        private Color _serverBtnColour;
        private string _serverBtnText = "START SERVER";

        // Controls button
        private Vector2 _controlsBtnPosition;
        private Rectangle _controlsBtnRect;
        private Color _controlsBtnColour;
        private string _controlsBtnText = "SET CONTROLS";

        private GraphicsDevice _device;

        private int _bindingStep = 0;
        private Keys _leftKey, _rightKey, _upKey, _downKey, _fireKey;
        private bool _bindingActive = false;

        private KeyboardState _prevKeyState;

        public MainMenu()
        {
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _device = graphicsDevice;
            _font = content.Load<SpriteFont>("Font");

            _viewPort = new Rectangle(Application.WINDOW_WIDTH / 2, 150, 400, 200);
            _textbox = new TextBox(_viewPort, 20, _textboxText, graphicsDevice, _font, Color.LightGray, Color.DarkGreen, 30);

            _titlePosition = new Vector2(Application.WINDOW_WIDTH / 2, 100);
            _titlePosition.X -= (_font.MeasureString(_titleText).X / 2);

            _connectBtnPosition = new Vector2(Application.WINDOW_WIDTH / 2, Application.WINDOW_HEIGHT - (_font.MeasureString(_connectBtnText).Y));
            _connectBtnPosition.X -= (_font.MeasureString(_connectBtnText).X / 2);
            _connectBtnPosition.Y -= 200;
            _connectBtnRect = new Rectangle((int)_connectBtnPosition.X, (int)_connectBtnPosition.Y,
                (int)_font.MeasureString(_connectBtnText).X, (int)_font.MeasureString(_connectBtnText).Y);
            _connectBtnColour = Color.Blue;

            _serverBtnPosition = new Vector2(Application.WINDOW_WIDTH / 2, Application.WINDOW_HEIGHT - (_font.MeasureString(_serverBtnText).Y));
            _serverBtnPosition.X -= (_font.MeasureString(_serverBtnText).X / 2);
            _serverBtnPosition.Y -= 150;
            _serverBtnRect = new Rectangle((int)_serverBtnPosition.X, (int)_serverBtnPosition.Y,
                (int)_font.MeasureString(_serverBtnText).X, (int)_font.MeasureString(_serverBtnText).Y);
            _serverBtnColour = Color.Blue;

            _controlsBtnPosition = new Vector2(Application.WINDOW_WIDTH / 2, Application.WINDOW_HEIGHT - (_font.MeasureString(_controlsBtnText).Y));
            _controlsBtnPosition.X -= (_font.MeasureString(_controlsBtnText).X / 2);
            _controlsBtnPosition.Y -= 100;
            _controlsBtnRect = new Rectangle((int)_controlsBtnPosition.X, (int)_controlsBtnPosition.Y,
                (int)_font.MeasureString(_controlsBtnText).X, (int)_font.MeasureString(_controlsBtnText).Y);
            _controlsBtnColour = Color.Blue;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw title
            spriteBatch.DrawString(_font, _titleText, _titlePosition, Color.White);

            // Draw connect button
            spriteBatch.DrawString(_font, _connectBtnText, _connectBtnPosition, Color.White);
            Texture2D connectBtnTexture = new Texture2D(_device, _connectBtnRect.Width, _connectBtnRect.Height);
            connectBtnTexture.CreateBorder(1, _connectBtnColour);
            spriteBatch.Draw(connectBtnTexture, _connectBtnPosition, Color.White);

            // Draw server button
            spriteBatch.DrawString(_font, _serverBtnText, _serverBtnPosition, Color.White);
            Texture2D serverBtnTexture = new Texture2D(_device, _serverBtnRect.Width, _serverBtnRect.Height);
            serverBtnTexture.CreateBorder(1, _serverBtnColour);
            spriteBatch.Draw(serverBtnTexture, _serverBtnPosition, Color.White);

            // Draw controls button
            spriteBatch.DrawString(_font, _controlsBtnText, _controlsBtnPosition, Color.White);
            Texture2D controlsBtnTexture = new Texture2D(_device, _controlsBtnRect.Width, _controlsBtnRect.Height);
            controlsBtnTexture.CreateBorder(1, _controlsBtnColour);
            spriteBatch.Draw(controlsBtnTexture, _controlsBtnPosition, Color.White);

            // Draw key binding instructions if active
            if (_bindingActive)
            {
                string instruction = "";
                switch (_bindingStep)
                {
                    case 1: instruction = "Press a key for LEFT"; break;
                    case 2: instruction = "Press a key for RIGHT"; break;
                    case 3: instruction = "Press a key for UP"; break;
                    case 4: instruction = "Press a key for DOWN"; break;
                    case 5: instruction = "Press a key for FIRE"; break;
                }
                Vector2 instrPos = new Vector2(Application.WINDOW_WIDTH / 2, _controlsBtnPosition.Y + 40);
                instrPos.X -= (_font.MeasureString(instruction).X / 2);
                spriteBatch.DrawString(_font, instruction, instrPos, Color.Yellow);
            }

            _textbox.Draw(spriteBatch);
        }

        private void OpenKeyBindingDialog()
        {
            _bindingStep = 1;
            _bindingActive = true;
            _prevKeyState = Keyboard.GetState(); // Reset previous state
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            if (_bindingActive)
            {
                HandleKeyBindingInput(inputInfo);
                return;
            }
            UpdateTextbox(gameTime);
            UpdateButtons(inputInfo);
        }

        private void HandleKeyBindingInput(InputInformation inputInfo)
        {
            KeyboardState ks = inputInfo.CurrentKeyboardState;
            Keys[] pressed = ks.GetPressedKeys();

            // Only register a key if it was not pressed in the previous frame
            foreach (var key in pressed)
            {
                if (Array.IndexOf(_prevKeyState.GetPressedKeys(), key) == -1)
                {
                    switch (_bindingStep)
                    {
                        case 1:
                            _leftKey = key;
                            _bindingStep++;
                            break;
                        case 2:
                            _rightKey = key;
                            _bindingStep++;
                            break;
                        case 3:
                            _upKey = key;
                            _bindingStep++;
                            break;
                        case 4:
                            _downKey = key;
                            _bindingStep++;
                            break;
                        case 5:
                            _fireKey = key;
                            _bindingStep = 0;
                            _bindingActive = false;
                            ApplyKeyBindings();
                            break;
                    }
                    break; // Only process one key per frame
                }
            }

            _prevKeyState = ks;
        }

        private void ApplyKeyBindings()
        {
            // You need a reference to your GameScene instance to call BindKey.
            // For demo, raise an event with the selected keys:
            OnControlsConfigured?.Invoke(_leftKey, _rightKey, _upKey, _downKey, _fireKey);
        }

        // Add this event to MainMenu
        public delegate void ControlsConfiguredDelegate(Keys left, Keys right, Keys up, Keys down, Keys fire);
        public static event ControlsConfiguredDelegate OnControlsConfigured;

        public void Update(GameTime gameTime)
        {
            _viewPort.X = (Application.WINDOW_WIDTH / 2) - (int)(_font.MeasureString(_textbox.Text.String).X / 2);
        }

        private void UpdateTextbox(GameTime gameTime)
        {
            KeyboardInput.Update();

            // This code is in here to play with in debug mode.
            // You may as well initialize it in LoadContent() or change it in here to achieve lerp effects and so on...
            float margin = 3;
            _textbox.Area = new Rectangle((int)(_viewPort.X + margin), _viewPort.Y, (int)(_viewPort.Width - margin),
                _viewPort.Height);
            _textbox.Renderer.Color = Color.White;
            _textbox.Cursor.Selection = new Color(Color.Purple, .4f);

            float lerpAmount = (float)(gameTime.TotalGameTime.TotalMilliseconds % 500f / 500f);
            _textbox.Cursor.Color = Color.Lerp(Color.DarkGray, Color.LightGray, lerpAmount);

            _textbox.Active = true;
            _textbox.Update();
        }

        private void UpdateButtons(InputInformation inputInfo)
        {
            if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released) // Clicked
            {
                if (_connectBtnRect.Contains(inputInfo.CurrentMouseState.Position))
                {
                    if (!string.IsNullOrEmpty(_textbox.Text.String))
                    {
                        // START GAME AS CLIENT
                        OnClientStartRequested(_textbox.Text.String);
                    }
                }
                else if (_serverBtnRect.Contains(inputInfo.CurrentMouseState.Position))
                {
                    // START GAME AS SERVER
                    OnServerStartRequested("str");
                }
                else if (_controlsBtnRect.Contains(inputInfo.CurrentMouseState.Position))
                {
                    // Open key binding dialog
                    OpenKeyBindingDialog();
                }
            }
            else
            {
                if (_connectBtnRect.Contains(inputInfo.CurrentMouseState.Position))
                {
                    if (string.IsNullOrEmpty(_textbox.Text.String))
                    {
                        _connectBtnColour = Color.Red;
                    }
                    else
                    {
                        _connectBtnColour = Color.LightGreen;
                    }
                }
                else
                {
                    _connectBtnColour = Color.Blue;
                }

                if (_serverBtnRect.Contains(inputInfo.CurrentMouseState.Position))
                {
                    _serverBtnColour = Color.LightGreen;
                }
                else
                {
                    _serverBtnColour = Color.Blue;
                }

                if (_controlsBtnRect.Contains(inputInfo.CurrentMouseState.Position))
                {
                    _controlsBtnColour = Color.LightGreen;
                }
                else
                {
                    _controlsBtnColour = Color.Blue;
                }
            }
        }
    }
}
