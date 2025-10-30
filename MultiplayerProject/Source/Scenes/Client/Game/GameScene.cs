﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiplayerProject.Source.Helpers.Factories;
using MultiplayerProject.Source.Helpers.Audio;
using System.Collections.Generic; // Add this line
using System.Linq;
using System;

namespace MultiplayerProject.Source
{
    public class GameScene : IScene
    {
        private Dictionary<string, IPlayer> _players;
        private Dictionary<string, string> _playerNames;
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
        
        // Font for drawing player names
        private SpriteFont _font;

        private int framesSinceLastSend;

        private int _packetNumber = -1;
        private Queue<PlayerUpdatePacket> _updatePackets;

        public Client Client { get; set; }

        private int _localPlayerScore;

        public GameScene(int width, int height, int playerCount, string[] playerIDs, string[] playerNames, PlayerColour[] playerColours, string localClientID, Client client)
        {
            _players = new Dictionary<string, IPlayer>();
            _playerNames = new Dictionary<string, string>();
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
                player.PlayerName = playerNames[i]; // Set the player name
                _playerColours[player.NetworkID] = playerColours[i];
                _playerNames[player.NetworkID] = playerNames[i]; // Store player names

                // Wrap all players with NameTagDecorator by default to show names
                IPlayer decoratedPlayer = new NameTagDecorator(player, showEnhancements: true);
                _players.Add(player.NetworkID, decoratedPlayer);
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

        /// <summary>
        /// Helper method to unwrap decorators and get the base Player object
        /// </summary>
        private T GetBasePlayer<T>(IPlayer decoratedPlayer) where T : Player
        {
            IPlayer current = decoratedPlayer;
            
            // Unwrap decorators to find the base player
            while (current is PlayerDecorator decorator)
            {
                current = decorator.WrappedPlayer;
            }
            
            return current as T;
        }

        /// <summary>
        /// Helper method to find a specific decorator type in the decorator chain
        /// </summary>
        private T FindDecorator<T>(IPlayer decoratedPlayer) where T : PlayerDecorator
        {
            IPlayer current = decoratedPlayer;
            
            // Walk through the decorator chain to find the specific type
            while (current is PlayerDecorator decorator)
            {
                if (decorator is T targetDecorator)
                {
                    return targetDecorator;
                }
                current = decorator.WrappedPlayer;
            }
            
            return null;
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            // Load font for player names
            _font = content.Load<SpriteFont>("Font");
            
            foreach (KeyValuePair<string, IPlayer> player in _players)
            {
                //player.Value.Initialize(content, _playerColours[player.Key]);
                player.Value.Initialize(content);
            }

            _GUI.Initalise(content);

            _enemyManager.Initalise(content);
            _laserManager.Initalise(content);
            _explosionManager.Initalise(content);
            _backgroundManager.Initalise(content);
            
            // Set default key bindings ONLY if none exist
            if (_keyCommandMap == null || _keyCommandMap.Count == 0)
            {
                InitializeDefaultKeyBindings();
            }

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
            
            // Update audio based on local player score
            if (_localPlayer != null && _GUI != null)
            {
                int currentScore = _GUI.GetLocalPlayerScore();
                _audioController.Update(currentScore);
                
                // Update shield status based on score
                UpdatePlayerShields();
                
                // Update rapid fire status based on score  
                UpdatePlayerRapidFire(currentScore);
                
                // Update fire rate based on score (keeping existing for compatibility)
                _laserManager.UpdateFireRate(currentScore);
            }
        }
        
        private void UpdatePlayerShields()
        {
            // Give shield to players who reach score of 5 or higher
            const int SHIELD_SCORE_THRESHOLD = 5;
            
            foreach (var kvp in _players.ToList()) // ToList to avoid modification during iteration
            {
                var player = kvp.Value;
                int playerScore = _GUI.GetPlayerScore(player.NetworkID);
                
                // Check if player should have shield but doesn't have one yet
                if (playerScore >= SHIELD_SCORE_THRESHOLD)
                {
                    // Look for existing ShieldDecorator in the decorator chain
                    var existingShield = FindDecorator<ShieldDecorator>(player);
                    
                    if (existingShield == null)
                    {
                        // No shield exists, add one while preserving NameTagDecorator as outermost
                        if (player is NameTagDecorator nameTagDecorator)
                        {
                            // Extract the player beneath the NameTagDecorator
                            var playerBelowNameTag = nameTagDecorator.WrappedPlayer;
                            var newShieldPlayer = new ShieldDecorator(playerBelowNameTag);
                            
                            // Rebuild: Base -> Shield -> NameTag
                            _players[kvp.Key] = new NameTagDecorator(newShieldPlayer, showEnhancements: true);
                        }
                        else
                        {
                            // No NameTagDecorator present, add Shield and then NameTag
                            var newShieldPlayer = new ShieldDecorator(player);
                            _players[kvp.Key] = new NameTagDecorator(newShieldPlayer, showEnhancements: true);
                        }
                    }
                    // If shield already exists, do nothing (prevents infinite updates)
                }
                // Note: We don't remove shields when score drops to keep it simple
                // In a full implementation, you might want to track shield state differently
            }
        }
        
        private void UpdatePlayerRapidFire(int localPlayerScore)
        {
            // Apply rapid fire decorator to local player based on score
            if (_localPlayer != null && localPlayerScore > 0)
            {
                // Check if the local player needs rapid fire enhancement
                var currentPlayer = _players[_localPlayer.NetworkID];
                
                // Look for existing RapidFireDecorator in the decorator chain
                var existingRapidFire = FindDecorator<RapidFireDecorator>(currentPlayer);
                
                if (existingRapidFire != null)
                {
                    // Check if the fire rate needs updating (score changed significantly)
                    float expectedMultiplier = 1.0f + (localPlayerScore / 3) * 0.25f;
                    expectedMultiplier = System.Math.Min(expectedMultiplier, 3.0f);
                    
                    if (System.Math.Abs(existingRapidFire.GetFireRateMultiplier() - expectedMultiplier) > 0.1f)
                    {
                        // Score changed significantly, reapply rapid fire with new multiplier
                        var basePlayer = GetBasePlayer<Player>(currentPlayer);
                        
                        // Rebuild the decoration chain: Base -> RapidFire -> NameTag (to preserve name display)
                        var newRapidFirePlayer = RapidFireDecorator.FromScore(basePlayer, localPlayerScore);
                        _players[_localPlayer.NetworkID] = new NameTagDecorator(newRapidFirePlayer, showEnhancements: true);
                    }
                    // If multiplier is close enough, do nothing (prevents infinite updates)
                }
                else
                {
                    // Apply rapid fire decorator for the first time
                    // Check if currentPlayer is already a NameTagDecorator to preserve it
                    if (currentPlayer is NameTagDecorator nameTagDecorator)
                    {
                        // Extract the player beneath the NameTagDecorator
                        var playerBelowNameTag = nameTagDecorator.WrappedPlayer;
                        var newRapidFirePlayer = RapidFireDecorator.FromScore(playerBelowNameTag, localPlayerScore);
                        
                        // Rebuild: Base -> RapidFire -> NameTag
                        _players[_localPlayer.NetworkID] = new NameTagDecorator(newRapidFirePlayer, showEnhancements: true);
                    }
                    else
                    {
                        // No NameTagDecorator present, just add RapidFire and then NameTag
                        var newRapidFirePlayer = RapidFireDecorator.FromScore(currentPlayer, localPlayerScore);
                        _players[_localPlayer.NetworkID] = new NameTagDecorator(newRapidFirePlayer, showEnhancements: true);
                    }
                }
            }
        }
        
        /// <summary>
        /// Helper method to unwrap decorators and get the base player
        /// This is a simplified version - in a full implementation you might want a more sophisticated approach
        /// </summary>
        private IPlayer GetBasePlayer(IPlayer decoratedPlayer)
        {
            // For now, just return the decorated player as we're keeping decorations
            // In a full implementation, you might want to unwrap to the actual base Player
            return decoratedPlayer;
        }
        
        /// <summary>
        /// Test method to demonstrate decorator pattern usage
        /// </summary>
        private void TestDecoratorPattern()
        {
            // Example: Create a player with multiple decorators
            if (_localPlayer != null)
            {
                IPlayer basePlayer = _localPlayer;
                
                // Apply decorators in sequence
                IPlayer decoratedPlayer = new NameTagDecorator(
                    new RapidFireDecorator(
                        new ShieldDecorator(basePlayer),
                        2.0f, 15f
                    ), 
                    showEnhancements: true
                );
                
                // Test that the player has all enhancements
                bool hasShield = decoratedPlayer.GetHasShield();
                float fireRate = decoratedPlayer.GetFireRateMultiplier();
                
                Logger.Instance?.Info($"Decorated player - Shield: {hasShield}, Fire Rate: {fireRate}x");
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _backgroundManager.Draw(spriteBatch);

            _enemyManager.Draw(spriteBatch);

            _laserManager.Draw(spriteBatch);

            foreach (KeyValuePair<string, IPlayer> player in _players)
            {
                // Use the new Draw method that shows player names
                player.Value.Draw(spriteBatch, _font);
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

        // 1. Command interface and concrete commands
        public interface IInputCommand
        {
            void Execute(KeyboardMovementInput input);
        }

        public class MoveLeftCommand : IInputCommand
        {
            public void Execute(KeyboardMovementInput input) => input.LeftPressed = true;
        }

        public class MoveRightCommand : IInputCommand
        {
            public void Execute(KeyboardMovementInput input) => input.RightPressed = true;
        }

        public class MoveUpCommand : IInputCommand
        {
            public void Execute(KeyboardMovementInput input) => input.UpPressed = true;
        }

        public class MoveDownCommand : IInputCommand
        {
            public void Execute(KeyboardMovementInput input) => input.DownPressed = true;
        }

        public class FireCommand : IInputCommand
        {
            public void Execute(KeyboardMovementInput input) => input.FirePressed = true;
        }

        // 2. Add a mapping from Keys to commands
        private Dictionary<Keys, IInputCommand> _keyCommandMap = new Dictionary<Keys, IInputCommand>();

        // 3. Initialize default mapping (call in constructor or Initalise)
        private void InitializeDefaultKeyBindings()
        {
            _keyCommandMap = new Dictionary<Keys, IInputCommand>
            {
                { Keys.Left, new MoveLeftCommand() },
                { Keys.Right, new MoveRightCommand() },
                { Keys.Up, new MoveUpCommand() },
                { Keys.Down, new MoveDownCommand() },
                { Keys.Space, new FireCommand() }
                // Add more or allow remapping
            };
        }

        // 4. Refactor ProcessInputForLocalPlayer to use commands
        private KeyboardMovementInput ProcessInputForLocalPlayer(GameTime gameTime, InputInformation inputInfo)
        {
            IGameInput inputAdapter;

            if(inputInfo.CurrentGamePadState.IsConnected)
            {
                inputAdapter = new GamepadAdapter();
            }
            else
            {
                inputAdapter = new KeyboardAdapter(_keyCommandMap);
            }

            KeyboardMovementInput input = inputAdapter.GetMovementInput(inputInfo);

            // Fire logic (unchanged, but now uses input.FirePressed)
            if (input.FirePressed)
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
                RemotePlayer remotePlayer = GetBasePlayer<RemotePlayer>(_players[serverUpdate.PlayerID]);
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

        // Add this method to GameScene
        public void BindKey(Keys key, IInputCommand command)
        {
            _keyCommandMap[key] = command;
        }
    }
}