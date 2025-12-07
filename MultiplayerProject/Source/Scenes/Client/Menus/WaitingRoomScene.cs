using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiplayerProject.Source.Networking.Chat;

namespace MultiplayerProject.Source
{
    public class GameRoomUIItem
    {
        public Vector2 Position;
        public Rectangle Rect;
        public string Text;
        public Color BorderColour;
        public int BorderWidth;
    }

    public enum WaitingRoomState
    {
        NotInRoomAbleToCreate,
        NotInRoomUnableToCreate,
        InRoomWaitingForPlayers,
        InRoomNotReady,
        InRoomReady,
    }

    public class WaitingRoomScene : IScene
    {
        private WaitingRoomInformation _waitingRoom;

        private string _joinedRoomID;
        private WaitingRoomState _state;
        private bool _readyToPlay;

        private SpriteFont _font;
        private GraphicsDevice _device;
        private bool _waitingForResponseFromServer;

        public Dictionary<string, string> ClientIdToNameMap = new Dictionary<string, string>();


        // Title
        private Vector2 _titlePosition;
        private const string _titleText = "WAITING ROOM";

        // Bottom button
        private Vector2 _buttonPosition;
        private Rectangle _buttonRect;
        private Color _buttonColour;
        private string _buttonText = "CREATE NEW ROOM";

        // Rooms
        private List<GameRoomUIItem> _roomUIItems;
        private const int _roomStartYPos = 50;

        private List<string> _chatMessages = new List<string>();
        private string _currentChatInput = "";

        public Client Client { get; set; }

       public WaitingRoomScene(Client client)
{
    Client = client;

    // Subscribe to waiting room updates
    Client.OnWaitingRoomInformationRecieved += Client_OnWaitingRoomInformationRecieved;

    _waitingForResponseFromServer = false;
    _state = WaitingRoomState.NotInRoomAbleToCreate;

    _roomUIItems = new List<GameRoomUIItem>();
}


        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Font");
            _device = graphicsDevice;

            _titlePosition = new Vector2(Application.WINDOW_WIDTH / 2, 0);
            _titlePosition.X -= (_font.MeasureString(_titleText).X / 2);

            ReformatButton();
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            if (_waitingForResponseFromServer)
                return;

            CheckBottomButtonClicked(inputInfo);

            CheckRoomClicked(inputInfo);

Keys[] pressedKeys = inputInfo.CurrentKeyboardState.GetPressedKeys();
Keys[] previousKeys = inputInfo.PreviousKeyboardState.GetPressedKeys();

foreach (var key in pressedKeys)
{
    if (Array.IndexOf(previousKeys, key) == -1) // Only new key presses
    {
        bool shift = inputInfo.CurrentKeyboardState.IsKeyDown(Keys.LeftShift) || inputInfo.CurrentKeyboardState.IsKeyDown(Keys.RightShift);

        if (key == Keys.Back && _currentChatInput.Length > 0)
        {
            _currentChatInput = _currentChatInput.Substring(0, _currentChatInput.Length - 1);
        }
        else if (key == Keys.Space)
        {
            _currentChatInput += " ";
        }
        else if (key >= Keys.A && key <= Keys.Z)
        {
            char c = (char)((shift ? 'A' : 'a') + (key - Keys.A));
            _currentChatInput += c;
        }
        else if (key >= Keys.D0 && key <= Keys.D9)
        {
            // Top-row numbers with shift for symbols
            char c = shift ? ")!@#$%^&*("[key - Keys.D0] : (char)('0' + (key - Keys.D0));
            _currentChatInput += c;
        }
        else if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
        {
            _currentChatInput += (char)('0' + (key - Keys.NumPad0));
        }
        else
        {
            // Special keys
            switch (key)
            {
                case Keys.OemPeriod:
                    _currentChatInput += ".";
                    break;
                case Keys.OemMinus:
                    _currentChatInput += shift ? "_" : "-";
                    break;
                case Keys.OemQuestion:
                    _currentChatInput += shift ? "?" : "/";
                    break;
                case Keys.OemComma:
                    _currentChatInput += shift ? "<" : ",";
                    break;
                case Keys.OemSemicolon:
                    _currentChatInput += shift ? ":" : ";";
                    break;
                case Keys.OemQuotes:
                    _currentChatInput += shift ? "\"" : "'";
                    break;
                case Keys.OemPlus:
                    _currentChatInput += shift ? "+" : "=";
                    break;
                case Keys.OemPipe:
                    _currentChatInput += shift ? "|" : "\\";
                    break;
            }
        }
    }
}




       if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Enter) && !string.IsNullOrWhiteSpace(_currentChatInput))
{
    string message = _currentChatInput.Trim();

   if (message.StartsWith("dm ", StringComparison.OrdinalIgnoreCase))
{
    // Syntax: dm <recipientName> <message>
    string[] parts = message.Split(new char[] { ' ' }, 3);
    if (parts.Length < 3)
    {
        _chatMessages.Add("[System] Invalid DM format. Use: dm <player> <message>");
    }
    else
    {
        string receiverName = parts[1];
        string dmMessage = parts[2];

       // Find receiver ID by name
// Try to get receiver ID from the dictionary
if (ClientIdToNameMap.TryGetValue(receiverName, out string receiverId))
{
    var packet = new ChatMessagePacket
    {
        Type = ChatMessageType.Private,
        SenderId = Client.ClientId,
        SenderName = Client.Name,
        ReceiverId = receiverId,
        Message = dmMessage
    };
    SendMessageToTheServer(packet, MessageType.ChatMessage);
}
else
{
    _chatMessages.Add($"[System] Player '{receiverName}' not found.");
}


if (receiverId != null)
{
    var packet = new ChatMessagePacket
    {
        Type = ChatMessageType.Private,
        SenderId = Client.ClientId,
        SenderName = Client.Name,
        ReceiverId = receiverId,
        Message = dmMessage
    };
    SendMessageToTheServer(packet, MessageType.ChatMessage);
}
else
{
    _chatMessages.Add($"[System] Player '{receiverName}' not found.");
}

    }
}

    else
    {
        // Global message
        var packet = new ChatMessagePacket
        {
            Type = ChatMessageType.Global,
            SenderId = Client.ClientId,
            SenderName = Client.Name,
            Message = message
        };
        SendMessageToTheServer(packet, MessageType.ChatMessage);
    }

    _currentChatInput = "";
}


        }

        public void Update(GameTime gameTime)
        {

        }
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw title
            spriteBatch.DrawString(_font, _titleText, _titlePosition, Color.White);

            // Draw new room button
            spriteBatch.DrawString(_font, _buttonText, _buttonPosition, Color.White);
            Texture2D newRoomBtnTexture = new Texture2D(_device, _buttonRect.Width, _buttonRect.Height);
            newRoomBtnTexture.CreateBorder(1, _buttonColour);
            spriteBatch.Draw(newRoomBtnTexture, _buttonPosition, Color.White);

            if (_roomUIItems.Count != 0)
            {
                for (int i = 0; i < _roomUIItems.Count; i++)
                {
                    // Draw room ui item string
                    spriteBatch.DrawString(_font, _roomUIItems[i].Text, _roomUIItems[i].Position, Color.White);

                    // Draw room ui item border
                    Texture2D texture = new Texture2D(_device, _roomUIItems[i].Rect.Width, _roomUIItems[i].Rect.Height);
                    texture.CreateBorder(_roomUIItems[i].BorderWidth, _roomUIItems[i].BorderColour);

                    if (_roomUIItems.Count > 0) // Fixes a bug where _roomUIItems[i] becomes null and crashes
                        spriteBatch.Draw(texture, _roomUIItems[i].Position, Color.White);
                }
            }

           int chatY = 400;
lock (_chatMessages)
{
    foreach (var msg in _chatMessages)
    {
        spriteBatch.DrawString(_font, msg, new Vector2(20, chatY), Color.White);
        chatY += 20;
    }
}
spriteBatch.DrawString(_font, "Chat: " + _currentChatInput, new Vector2(20, chatY), Color.LightGray);

        }

        public void RequestWaitingRoomUpdate()
        {
            SendMessageToTheServer(new BasePacket(), MessageType.WR_ClientRequest_WaitingRoomInfo);
        }

public void RecieveChatMessage(ChatMessagePacket chatPacket)
{
    lock (_chatMessages)
    {
        if (chatPacket.Type == ChatMessageType.Global)
    _chatMessages.Add($"[Global] {chatPacket.SenderName}: {chatPacket.Message}");
else if (chatPacket.Type == ChatMessageType.Private)
    _chatMessages.Add($"[DM] {chatPacket.SenderName} -> {chatPacket.ReceiverId}: {chatPacket.Message}");


        // Keep list manageable
        if (_chatMessages.Count > 50)
            _chatMessages.RemoveAt(0);
    }
}

        private void ClientMessenger_OnRoomSuccessfullyUnready()
        {
            _readyToPlay = false;
            _waitingForResponseFromServer = false;
            _state = WaitingRoomState.InRoomNotReady;
        }

        private void ClientMessenger_OnRoomSuccessfullyReady()
        {
            _readyToPlay = true;
            _waitingForResponseFromServer = false;
            _state = WaitingRoomState.InRoomReady;
        }

        private void ClientMessageReciever_OnRoomSuccessfullyLeft(string str)
        {
            _joinedRoomID = "";
            _waitingForResponseFromServer = false;
        }

        private void ClientMessageReciever_OnRoomSuccessfullyJoined(string str)
        {
            _joinedRoomID = str;
            _waitingForResponseFromServer = false;
        }

private void Client_OnWaitingRoomInformationRecieved(WaitingRoomInformation packet)
{
    // Update local waiting room info
    _waitingRoom = packet;

    // Clear previous mapping
    ClientIdToNameMap.Clear();

    // Populate ClientIdToNameMap from all rooms
    if (_waitingRoom != null && _waitingRoom.Rooms != null)
    {
        foreach (var room in _waitingRoom.Rooms)
        {
            if (room.ConnectionIDs != null && room.ConnectionNames != null)
            {
                for (int i = 0; i < room.ConnectionIDs.Length && i < room.ConnectionNames.Length; i++)
                {
                    string clientId = room.ConnectionIDs[i];
                    string playerName = room.ConnectionNames[i];

                    // Only add non-empty names
                    if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(playerName))
                        ClientIdToNameMap[clientId] = playerName;
                }
            }
        }
    }

    // DEBUG: display all players in the console
    Console.WriteLine("=== Current players in rooms ===");
    if (ClientIdToNameMap.Count == 0)
    {
        Console.WriteLine("No players found!");
    }
    else
    {
        foreach (var kvp in ClientIdToNameMap)
        {
            Console.WriteLine($"ID: {kvp.Key}, Name: {kvp.Value}");
        }
    }
    Console.WriteLine("================================");

    // Build UI room list
    List<GameRoomUIItem> newItems = new List<GameRoomUIItem>();
    RoomInformation joinedRoom = null;

    if (_waitingRoom != null && _waitingRoom.Rooms != null)
    {
        int startYPos = _roomStartYPos;

        foreach (var room in _waitingRoom.Rooms)
        {
            GameRoomUIItem uiItem = new GameRoomUIItem();
            uiItem.Rect = new Rectangle(50, startYPos, 500, 50);

            switch ((GameRoomState)room.RoomState)
            {
                case GameRoomState.Waiting:
                    uiItem.Text = room.RoomName + " : " + room.ConnectionCount + "/" + WaitingRoom.MAX_PEOPLE_PER_ROOM + " Players";
                    break;
                case GameRoomState.InSession:
                    uiItem.Text = room.RoomName + " : PLAYING";
                    break;
                case GameRoomState.Leaderboards:
                    uiItem.Text = room.RoomName + " : GAME FINISHING";
                    break;
            }

            Vector2 size = _font.MeasureString(uiItem.Text);
            float xScale = (uiItem.Rect.Width / size.X);
            float yScale = (uiItem.Rect.Height / size.Y);
            float scale = Math.Min(xScale, yScale);

            int strWidth = (int)Math.Round(size.X * scale);
            int strHeight = (int)Math.Round(size.Y * scale);

            uiItem.Position = new Vector2
            {
                X = (((uiItem.Rect.Width - strWidth) / 2) + uiItem.Rect.X),
                Y = (((uiItem.Rect.Height - strHeight) / 2) + uiItem.Rect.Y)
            };

            uiItem.BorderColour = Color.Blue;
            uiItem.BorderWidth = 1;

            newItems.Add(uiItem);
            startYPos += 50;

            if (room.RoomID == _joinedRoomID)
                joinedRoom = room;
        }
    }

    _roomUIItems = newItems;

    // Update bottom button text based on joined room state
    if (joinedRoom != null)
    {
        if (joinedRoom.ConnectionCount == 1)
        {
            _buttonText = "WAITING FOR MORE PLAYERS";
            _state = WaitingRoomState.InRoomWaitingForPlayers;
            _readyToPlay = false;
        }
        else
        {
            if (!_readyToPlay)
            {
                _buttonText = "CLICK TO READY (" + joinedRoom.ReadyCount + "/" + joinedRoom.ConnectionCount + ")";
                _state = WaitingRoomState.InRoomNotReady;
            }
            else
            {
                _buttonText = "READY TO PLAY! (" + joinedRoom.ReadyCount + "/" + joinedRoom.ConnectionCount + ")";
                _state = WaitingRoomState.InRoomReady;
            }
        }
    }
    else
    {
        if (_roomUIItems.Count < Server.MAX_ROOMS)
        {
            _buttonText = "CREATE NEW ROOM";
            _state = WaitingRoomState.NotInRoomAbleToCreate;
        }
        else
        {
            _buttonText = "UNABLE TO CREATE NEW ROOM";
            _state = WaitingRoomState.NotInRoomUnableToCreate;
        }
    }

    ReformatButton();
}


        private void ReformatButton()
        {
            _buttonPosition = new Vector2(Application.WINDOW_WIDTH / 2, Application.WINDOW_HEIGHT - (_font.MeasureString(_buttonText).Y));
            _buttonPosition.X -= (_font.MeasureString(_buttonText).X / 2);
            _buttonRect = new Rectangle((int)_buttonPosition.X, (int)_buttonPosition.Y,
                (int)_font.MeasureString(_buttonText).X, (int)_font.MeasureString(_buttonText).Y);
            _buttonColour = Color.Blue;
        }

        private void CheckRoomClicked(InputInformation inputInfo)
        {
            switch (_state)
            {
                case WaitingRoomState.NotInRoomUnableToCreate:
                case WaitingRoomState.NotInRoomAbleToCreate:
                    {
                        if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                        {
                            for (int i = 0; i < _roomUIItems.Count; i++)
                            {
                                if (_roomUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position) && (GameRoomState)_waitingRoom.Rooms[i].RoomState != GameRoomState.InSession && (GameRoomState)_waitingRoom.Rooms[i].RoomState != GameRoomState.Leaderboards)
                                {
                                    SendMessageToTheServer(NetworkPacketFactory.Instance.MakeStringPacket(_waitingRoom.Rooms[i].RoomID), MessageType.WR_ClientRequest_JoinRoom);
                                    _waitingForResponseFromServer = true;
                                }
                            }
                        }
                        else if (inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                        {
                            for (int i = 0; i < _roomUIItems.Count; i++)
                            {
                                if ((GameRoomState)_waitingRoom.Rooms[i].RoomState == GameRoomState.InSession || (GameRoomState)_waitingRoom.Rooms[i].RoomState == GameRoomState.Leaderboards)
                                {
                                    _roomUIItems[i].BorderColour = Color.Red;
                                }
                                else if (_roomUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position))
                                {
                                    _roomUIItems[i].BorderColour = Color.LightGreen;
                                }
                                else
                                {
                                    _roomUIItems[i].BorderColour = Color.Blue;
                                }
                            }
                        }
                        break;
                    }

                case WaitingRoomState.InRoomWaitingForPlayers:
                case WaitingRoomState.InRoomNotReady:
                    {
                        if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                        {
                            for (int i = 0; i < _roomUIItems.Count; i++)
                            {
                                if (_waitingRoom.Rooms[i].RoomID == _joinedRoomID)
                                {
                                    if (_roomUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position))
                                    {
                                        SendMessageToTheServer(NetworkPacketFactory.Instance.MakeStringPacket(_waitingRoom.Rooms[i].RoomID), MessageType.WR_ClientRequest_LeaveRoom);
                                        _waitingForResponseFromServer = true;
                                    }
                                }
                            }
                        }
                        else if (inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                        {
                            for (int i = 0; i < _roomUIItems.Count; i++)
                            {
                                if (_waitingRoom.Rooms[i].RoomID == _joinedRoomID)
                                {
                                    if (_roomUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position))
                                    {
                                        _roomUIItems[i].BorderColour = Color.Orange;
                                    }
                                    else
                                    {
                                        _roomUIItems[i].BorderColour = Color.LightGreen;
                                    }
                                }
                                else
                                {
                                    if (_roomUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position))
                                    {
                                        _roomUIItems[i].BorderColour = Color.Red;
                                    }
                                    else
                                    {
                                        _roomUIItems[i].BorderColour = Color.Blue;
                                    }
                                }
                            }
                        }
                        break;
                    }

                case WaitingRoomState.InRoomReady:
                    {
                        for (int i = 0; i < _roomUIItems.Count; i++)
                        {
                            if (_waitingRoom.Rooms[i].RoomID == _joinedRoomID)
                            {
                                _roomUIItems[i].BorderColour = Color.LightGreen;
                            }
                            else
                            {
                                _roomUIItems[i].BorderColour = Color.Blue;
                            }
                        }
                        break;
                    }
            }
        }

        private void CheckBottomButtonClicked(InputInformation inputInfo)
        {
            switch (_state)
            {
                case WaitingRoomState.NotInRoomAbleToCreate:
                    {
                        // Is it possible to create a new room? Must be less rooms than max and this client can't currently be in a room
                        if (_roomUIItems.Count < Server.MAX_ROOMS && string.IsNullOrEmpty(_joinedRoomID))
                        {
                            // If mouse has been clicked
                            if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                            {
                                if (_buttonRect.Contains(inputInfo.CurrentMouseState.Position))
                                {
                                    // New room valid click
                                    SendMessageToTheServer(new BasePacket(), MessageType.WR_ClientRequest_CreateRoom);
                                }
                            }
                            else if (inputInfo.PreviousMouseState.LeftButton == ButtonState.Released) // Else if mouse is hovering
                            {
                                if (_buttonRect.Contains(inputInfo.CurrentMouseState.Position))
                                {
                                    _buttonColour = Color.LightGreen;
                                }
                                else
                                {
                                    _buttonColour = Color.Blue;
                                }
                            }
                        }
                        else
                        {
                            _buttonColour = Color.Red;
                        }
                        break;
                    }

                case WaitingRoomState.NotInRoomUnableToCreate:
                    {
                        _buttonColour = Color.Red;
                        break;
                    }

                case WaitingRoomState.InRoomWaitingForPlayers:
                    {
                        _buttonColour = Color.Red;
                        break;
                    }

                case WaitingRoomState.InRoomNotReady:
                    {
                        if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                        {
                            if (_buttonRect.Contains(inputInfo.CurrentMouseState.Position))
                            {
                                // ON READY UP
                                SendMessageToTheServer(new BasePacket(), MessageType.GR_ClientRequest_Ready);
                                _waitingForResponseFromServer = true;
                            }
                        }
                        else if (inputInfo.PreviousMouseState.LeftButton == ButtonState.Released) // Else if mouse is hovering
                        {
                            if (_buttonRect.Contains(inputInfo.CurrentMouseState.Position))
                            {
                                _buttonColour = Color.LightGreen;
                            }
                            else
                            {
                                _buttonColour = Color.Orange;
                            }
                        }
                        else
                        {
                            _buttonColour = Color.Orange;
                        }

                        break;
                    }

                case WaitingRoomState.InRoomReady:
                    {
                        if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                        {
                            if (_buttonRect.Contains(inputInfo.CurrentMouseState.Position))
                            {
                                // ON UNREADY
                                SendMessageToTheServer(new BasePacket(), MessageType.GR_ClientRequest_Unready);
                                _waitingForResponseFromServer = true;
                            }
                        }
                        else if (inputInfo.PreviousMouseState.LeftButton == ButtonState.Released) // Else if mouse is hovering
                        {
                            if (_buttonRect.Contains(inputInfo.CurrentMouseState.Position))
                            {
                                _buttonColour = Color.Orange;
                            }
                            else
                            {
                                _buttonColour = Color.LightGreen;
                            }
                        }
                        else
                        {
                            _buttonColour = Color.LightGreen;
                        }
                        break;
                    }
            }
        }

        public void RecieveServerResponse(BasePacket recievedPacket)
        {
            switch ((MessageType)recievedPacket.MessageType)
            {
                case MessageType.WR_ServerSend_WaitingRoomFullInfo:
                    var waitingRooms = (WaitingRoomInformation)recievedPacket;
                    Client_OnWaitingRoomInformationRecieved(waitingRooms);
                    break;

                case MessageType.WR_ServerResponse_FailJoinRoom:
                    Console.WriteLine("FAILED TO JOIN ROOM");
                    break;

                case MessageType.WR_ServerResponse_FailCreateRoom:
                    Console.WriteLine("FAILED TO CREATE ROOM");
                    break;

                case MessageType.WR_ServerResponse_SuccessJoinRoom:
                    {
                        StringPacket lobbyID = (StringPacket)recievedPacket;
                        ClientMessageReciever_OnRoomSuccessfullyJoined(lobbyID.String);
                        break;
                    }

                case MessageType.WR_ServerResponse_SuccessLeaveRoom:
                    {
                        StringPacket lobbyID = (StringPacket)recievedPacket;
                        ClientMessageReciever_OnRoomSuccessfullyLeft(lobbyID.String);
                        break;
                    }

                case MessageType.GR_ServerResponse_SuccessReady:
                    {
                        ClientMessenger_OnRoomSuccessfullyReady();
                        break;
                    }

                case MessageType.GR_ServerResponse_SuccessUnready:
                    {
                        ClientMessenger_OnRoomSuccessfullyUnready();
                        break;
                    }

               
            }
            // ✅ Chat handling goes OUTSIDE the switch
    if (recievedPacket is ChatMessagePacket chatPacket)
{
    lock (_chatMessages)
    {
        if (chatPacket.Type == ChatMessageType.Global)
            _chatMessages.Add($"[Global] {chatPacket.SenderId}: {chatPacket.Message}");
        else if (chatPacket.Type == ChatMessageType.Private)
            _chatMessages.Add($"[DM] {chatPacket.SenderId} -> {chatPacket.ReceiverId}: {chatPacket.Message}");

        // Optional: keep only last 50 messages
        if (_chatMessages.Count > 50)
            _chatMessages.RemoveAt(0);
    }
}

        }

        public void SendMessageToTheServer(BasePacket packet, MessageType messageType)
        {
            Client.SendMessageToServer(packet, messageType);
        }
    }
}
