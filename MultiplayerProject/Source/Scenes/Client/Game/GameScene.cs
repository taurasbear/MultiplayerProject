using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiplayerProject.Source.Helpers.Factories;
using MultiplayerProject.Source.Helpers.Audio;
using System.Collections.Generic; // Add this line

namespace MultiplayerProject.Source
{
    public class GameScene : IScene
    {
        private Dictionary<string, Player> _players;
        private List<RemotePlayer> _remotePlayers;
        private LocalPlayer _localPlayer;
        private Dictionary<string, PlayerColour> _playerColours;

        private GameSceneGUI _GUI;

        private EnemyManager _enemyManager;
        private LaserManager _laserManager;
        private ExplosionManager _explosionManager;
        private BackgroundManager _backgroundManager;
        
        // Add audio controller field
        private ScoreBasedAudioController _audioController;

        private int framesSinceLastSend;

        private int _packetNumber = -1;
        private Queue<PlayerUpdatePacket> _updatePackets;

        public Client Client { get; set; }

        private int _localPlayerScore;

        private int _audioUpdateCounter = 0;
        private const int AUDIO_UPDATE_FREQUENCY = 30; // Update audio every 30 frames (twice per second)

        public GameScene(int width, int height, int playerCount, string[] playerIDs, string[] playerNames, PlayerColour[] playerColours, string localClientID, Client client)
        {
            _players = new Dictionary<string, Player>();
            _playerColours = new Dictionary<string, PlayerColour>();
            _remotePlayers = new List<RemotePlayer>();

            Client = client;

            for (int i = 0; i < playerCount; i++)
            {
                Player player; // Add this declaration
                
                if (playerIDs[i] == localClientID)
                {
                    // Use factory to create local player
                    player = CreatePlayerFromColor(playerColours[i]);
                    _localPlayer = player as LocalPlayer;

                    // If the factory didn't create a LocalPlayer, wrap it
                    if (_localPlayer == null)
                    {
                        _localPlayer = new LocalPlayer();
                        _localPlayer.NetworkID = playerIDs[i];
                        _localPlayer.Colour = playerColours[i];
                        player = _localPlayer;
                    }
                }
                else
                {
                    // Use factory to create remote player
                    player = CreatePlayerFromColor(playerColours[i]);
                    var remotePlayer = player as RemotePlayer;

                    // If the factory didn't create a RemotePlayer, wrap it
                    if (remotePlayer == null)
                    {
                        remotePlayer = new RemotePlayer();
                        remotePlayer.NetworkID = playerIDs[i];
                        remotePlayer.Colour = playerColours[i];
                        player = remotePlayer;
                    }

                    _remotePlayers.Add(remotePlayer);
                }

                player.NetworkID = playerIDs[i];
                player.Colour = playerColours[i];
                _playerColours[player.NetworkID] = playerColours[i];

                _players.Add(player.NetworkID, player);
            }

            // Pass localClientID to GameSceneGUI
            _GUI = new GameSceneGUI(width, height, playerIDs, playerNames, playerColours, localClientID);

            _updatePackets = new Queue<PlayerUpdatePacket>();

            _enemyManager = new EnemyManager();
            _laserManager = new LaserManager();
            _backgroundManager = new BackgroundManager();

            _explosionManager = new ExplosionManager();
            
            // Initialize audio controller
            _audioController = new ScoreBasedAudioController();
        }
        private Player CreatePlayerFromColor(PlayerColour colour)
        {
            GameObjectFactory factory;

            // Determine which factory to use based on color
            // Convert PlayerColour to Color for comparison
            var color = new Color(colour.R, colour.G, colour.B);

            if (color == Color.Red)
            {
                factory = new RedFactory();
            }
            else if (color == Color.Blue)
            {
                factory = new BlueFactory();
            }
            else if (color == Color.Green)
            {
                factory = new GreenFactory();
            }
            else
            {
                // Default to Red factory for any other colors
                factory = new RedFactory();
            }

            return (Player)factory.GetPlayer();
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            foreach (KeyValuePair<string, Player> player in _players)
            {
                //player.Value.Initialize(content, _playerColours[player.Key]);
                player.Value.Initialize(content);
            }

            _GUI.Initalise(content);

            _enemyManager.Initalise(content);
            _laserManager.Initalise(content);
            _explosionManager.Initalise(content);
            _backgroundManager.Initalise(content);
            
            // Start the audio controller
            _audioController.Start();
            Logger.Instance.Info("Audio controller started in GameScene");
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < _remotePlayers.Count; i++)
            {
                _remotePlayers[i].UpdateRemote(Application.CLIENT_UPDATE_RATE, (float)gameTime.ElapsedGameTime.TotalSeconds);
                _remotePlayers[i].Update(gameTime);
            }

            _backgroundManager.Update(gameTime);
            _enemyManager.Update(gameTime);
            _laserManager.Update(gameTime);
            _explosionManager.Update(gameTime);
            
            // Update audio less frequently to prevent overload
            _audioUpdateCounter++;
            if (_audioUpdateCounter >= AUDIO_UPDATE_FREQUENCY && _localPlayer != null && _GUI != null)
            {
                int currentScore = _GUI.GetLocalPlayerScore();
                _audioController.Update(currentScore);
                _audioUpdateCounter = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _backgroundManager.Draw(spriteBatch);

            _enemyManager.Draw(spriteBatch);

            _laserManager.Draw(spriteBatch);

            foreach (KeyValuePair<string, Player> player in _players)
            {
                player.Value.Draw(spriteBatch);
            }

            _explosionManager.Draw(spriteBatch);

            _GUI.Draw(spriteBatch);
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            // Is it time to send outgoing network packets?
            bool sendPacketThisFrame = false;

            framesSinceLastSend++;

            if (framesSinceLastSend >= Application.CLIENT_UPDATE_RATE)
            {
                sendPacketThisFrame = true;
                framesSinceLastSend = 0;
            }

            // Process and fetch input from local player
            KeyboardMovementInput condensedInput = ProcessInputForLocalPlayer(gameTime, inputInfo);

            // Build an update packet from the input and player values
            PlayerUpdatePacket packet = _localPlayer.BuildUpdatePacket();
            packet.DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            packet.Input = condensedInput;
            packet.SequenceNumber = _packetNumber++;

            // Add it to the queue
            _updatePackets.Enqueue(packet);

            if (sendPacketThisFrame)
            {
                // Send the packet to the server
                SendMessageToTheServer(packet, MessageType.GI_ClientSend_PlayerUpdate);
            }
        }

        private KeyboardMovementInput ProcessInputForLocalPlayer(GameTime gameTime, InputInformation inputInfo)
        {
            KeyboardMovementInput input = new KeyboardMovementInput();

            // Keyboard/Dpad controls
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Left) || inputInfo.CurrentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                input.LeftPressed = true;
            }
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Right) || inputInfo.CurrentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                input.RightPressed = true;
            }
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Up) || inputInfo.CurrentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                input.UpPressed = true;
            }
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Down) || inputInfo.CurrentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                input.DownPressed = true;
            }

            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Space) || inputInfo.CurrentGamePadState.Buttons.X == ButtonState.Pressed)
            {
                var laser = _laserManager.FireLocalLaserClient(gameTime, _localPlayer.Position, _localPlayer.Rotation, _playerColours[_localPlayer.NetworkID]);
                if (laser != null)
                {
                    input.FirePressed = true;
                    
                    // Play laser sound with score-based dynamics
                    int currentScore = _GUI?.GetLocalPlayerScore() ?? 0;
                    _audioController?.PlayLaserSound(currentScore);
                    
                    var dataPacket = _localPlayer.BuildUpdatePacket();
                    PlayerFiredPacket packet = NetworkPacketFactory.Instance.MakePlayerFiredPacket(dataPacket.XPosition, dataPacket.YPosition, dataPacket.Speed, dataPacket.Rotation);
                    packet.TotalGameTime = (float)gameTime.TotalGameTime.TotalSeconds;
                    packet.LaserID = laser.LaserID;

                    // Send the packet to the server
                    SendMessageToTheServer(packet, MessageType.GI_ClientSend_PlayerFired);
                }
            }

            if (Application.APPLY_CLIENT_SIDE_PREDICTION)
            {
                _localPlayer.ApplyInputToPlayer(input, (float)gameTime.ElapsedGameTime.TotalSeconds);
                _localPlayer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            _localPlayer.Update(gameTime);

            return input;
        }

        private void OnRecievedPlayerUpdatePacket(BasePacket packet)
        {
            PlayerUpdatePacket serverUpdate = (PlayerUpdatePacket)packet;

            if (Application.APPLY_SERVER_RECONCILLIATION &&
                serverUpdate.PlayerID == _localPlayer.NetworkID && serverUpdate.SequenceNumber >= 0
                && _updatePackets.Count > 0)
            {

                PlayerUpdatePacket localUpdate = GetUpdateAtSequenceNumber(serverUpdate.SequenceNumber);

                if (localUpdate.XPosition != serverUpdate.XPosition
                    || localUpdate.YPosition != serverUpdate.YPosition
                    || localUpdate.Rotation != serverUpdate.Rotation
                    || localUpdate.Speed != serverUpdate.Speed)
                {
                    // Create a new queue with 'serverUpdate' as the first update
                    var newQueue = new Queue<PlayerUpdatePacket>();
                    var updateList = new List<PlayerUpdatePacket>();

                    PlayerUpdatePacket removedPacket = _updatePackets.Dequeue(); // Remove the first one which we are replacing with the serverUpdate

                    serverUpdate.DeltaTime = removedPacket.DeltaTime;
                    newQueue.Enqueue(serverUpdate);
                    updateList.Add(serverUpdate);

                    while (_updatePackets.Count > 0)
                    {
                        PlayerUpdatePacket updatePacket = _updatePackets.Dequeue();
                        newQueue.Enqueue(updatePacket);
                        updateList.Add(updatePacket);
                    }

                    _updatePackets = newQueue; // Set the new queue

                    if (updateList.Count == 0)
                        return;

                    _localPlayer.SetPlayerState(updateList[0]);
                    _localPlayer.Update(updateList[0].DeltaTime);

                    if (updateList.Count == 1)
                        return;

                    // Now we must perform the previous inputs again
                    for (int i = 1; i < updateList.Count; i++)
                    {
                        _localPlayer.ApplyInputToPlayer(updateList[i].Input, updateList[i].DeltaTime);
                        _localPlayer.Update(updateList[i].DeltaTime);
                    }
                }
            }
            else
            {
                RemotePlayer remotePlayer = _players[serverUpdate.PlayerID] as RemotePlayer;
                remotePlayer.SetUpdatePacket(serverUpdate);
            }
        }

        private void ClientMessenger_OnRecievedPlayerFiredPacket(BasePacket packet)
        {
            PlayerFiredPacket playerUpdate = (PlayerFiredPacket)packet;
            if (playerUpdate.PlayerID != _localPlayer.NetworkID) // Local laser has already been shot so don't shoot it again
            {
                _laserManager.FireRemoteLaserClient(new Vector2(playerUpdate.XPosition, playerUpdate.YPosition), playerUpdate.Rotation, playerUpdate.PlayerID, playerUpdate.SendDate, playerUpdate.LaserID, _playerColours[playerUpdate.PlayerID]);
            }
        }

        private void ClientMessenger_OnEnemySpawnedPacket(BasePacket packet)
        {
            EnemySpawnedPacket enemySpawn = (EnemySpawnedPacket)packet;
            _enemyManager.AddEnemy(new Vector2(enemySpawn.XPosition, enemySpawn.YPosition), enemySpawn.EnemyID);
        }

        private void ClientMessenger_OnEnemyDefeatedPacket(EnemyDefeatedPacket packet)
        {
            EnemyDefeatedPacket enemyDefeatedPacket = packet;

            _GUI.UpdatePlayerScore(enemyDefeatedPacket.AttackingPlayerID, enemyDefeatedPacket.AttackingPlayerNewScore);

            _laserManager.DeactivateLaser(enemyDefeatedPacket.CollidedLaserID);

            var enemy = _enemyManager.DeactivateAndReturnEnemy(enemyDefeatedPacket.CollidedEnemyID);

            // Use the color of the player who fired the laser
            PlayerColour pc = _playerColours[packet.AttackingPlayerID];
            Color playerColor = new Color(pc.R, pc.G, pc.B);
            _explosionManager.AddExplosion(enemy.Position, playerColor);
        }

        private void ClientMessenger_OnPlayerDefeatedPacket(BasePacket packet)
        {
            PlayerDefeatedPacket playerDefeatedPacket = (PlayerDefeatedPacket)packet;

            _GUI.UpdatePlayerScore(playerDefeatedPacket.CollidedPlayerID, playerDefeatedPacket.CollidedPlayerNewScore);

            _laserManager.DeactivateLaser(playerDefeatedPacket.CollidedLaserID);

            var player = _players[playerDefeatedPacket.CollidedPlayerID];

            // Use the color of the player who fired the laser
            PlayerColour pc = _playerColours[playerDefeatedPacket.CollidedPlayerID];
            Color playerColor = new Color(pc.R, pc.G, pc.B);
            _explosionManager.AddExplosion(player.Position, playerColor);
        }

        private PlayerUpdatePacket GetUpdateAtSequenceNumber(int sequenceNumber)
        {
            PlayerUpdatePacket localUpdate;

            while (true)
            {
                localUpdate = _updatePackets.Peek();
                if (localUpdate.SequenceNumber != sequenceNumber)
                {
                    _updatePackets.Dequeue();
                }
                else
                {
                    break;
                }
            }

            return localUpdate;
        }

        public void RecieveServerResponse(BasePacket recievedPacket)
        {
            switch ((MessageType)recievedPacket.MessageType)
            {
                case MessageType.GI_ServerSend_UpdateRemotePlayer:
                    {
                        var playerPacket = (PlayerUpdatePacket)recievedPacket;
                        OnRecievedPlayerUpdatePacket(playerPacket);
                        break;
                    }

                case MessageType.GI_ServerSend_RemotePlayerFired:
                    {
                        var playerPacket = (PlayerFiredPacket)recievedPacket;
                        ClientMessenger_OnRecievedPlayerFiredPacket(playerPacket);
                        break;
                    }

                case MessageType.GI_ServerSend_EnemySpawn:
                    {
                        var enemyPacket = (EnemySpawnedPacket)recievedPacket;
                        ClientMessenger_OnEnemySpawnedPacket(enemyPacket);
                        break;
                    }

                case MessageType.GI_ServerSend_EnemyDefeated:
                    {
                        var enemyPacket = (EnemyDefeatedPacket)recievedPacket;
                        ClientMessenger_OnEnemyDefeatedPacket(enemyPacket);
                        
                        // Play explosion sound with score-based dynamics
                        int currentScore = _GUI?.GetLocalPlayerScore() ?? 0;
                        _audioController?.PlayExplosionSound(currentScore);
                        break;
                    }

                case MessageType.GI_ServerSend_PlayerDefeated:
                    {
                        var enemyPacket = (PlayerDefeatedPacket)recievedPacket;
                        ClientMessenger_OnPlayerDefeatedPacket(enemyPacket);
                        break;
                    }
            }
        }

        public void SendMessageToTheServer(BasePacket packet, MessageType messageType)
        {
            Client.SendMessageToServer(packet, messageType);
        }
    }
}