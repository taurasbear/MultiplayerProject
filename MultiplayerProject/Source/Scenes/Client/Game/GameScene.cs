using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.Helpers.Factories;
using System;
using System.Collections.Generic;
using MultiplayerProject.Source.GameObjects.Enemy;

namespace MultiplayerProject.Source
{
    public class GameScene : IScene
    {
        private Dictionary<string, Player> _players;
        private List<RemotePlayer> _remotePlayers;
        private LocalPlayer _localPlayer;
        private Dictionary<string, PlayerColour> _playerColours;
        private Dictionary<string, ElementalType> _playerElements;

        private GameSceneGUI _GUI;

        private EnemyManager _enemyManager;
        private LaserManager _laserManager;
        private ExplosionManager _explosionManager;
        private BackgroundManager _backgroundManager;

        private int framesSinceLastSend;

        private int _packetNumber = -1;
        private Queue<PlayerUpdatePacket> _updatePackets;

        private int _enemySpawnCounter = 0; // Track how many enemies have been spawned

        public Client Client { get; set; }

        public GameScene(int width, int height, int playerCount, string[] playerIDs, string[] playerNames, PlayerColour[] playerColours, string localClientID, Client client)
        {
            _players = new Dictionary<string, Player>();
            _playerColours = new Dictionary<string, PlayerColour>();
            _playerElements = new Dictionary<string, ElementalType>();
            _remotePlayers = new List<RemotePlayer>();

            Client = client;

            for (int i = 0; i < playerCount; i++)
            {
                Player player;

                if (playerIDs[i] == localClientID)
                {
                    _localPlayer = new LocalPlayer();
                    player = _localPlayer;
                }
                else
                {
                    var remotePlayer = new RemotePlayer();
                    player = remotePlayer;
                    _remotePlayers.Add(remotePlayer);
                }

                player.NetworkID = playerIDs[i];
                // Set all player colors to white
                var whiteColour = new PlayerColour { R = 255, G = 255, B = 255 };
                _playerColours[player.NetworkID] = whiteColour;
                player.SetColour(whiteColour);

                // Assign elements in a fixed order: Fire, Water, Electric, Fire, ...
                ElementalType[] elementOrder = { ElementalType.Fire, ElementalType.Water, ElementalType.Electric, ElementalType.Fire };
                _playerElements[player.NetworkID] = elementOrder[i % elementOrder.Length];

                _players.Add(player.NetworkID, player);
            }

            _GUI = new GameSceneGUI(width, height, playerIDs, playerNames, playerColours);

            _updatePackets = new Queue<PlayerUpdatePacket>();

            _enemyManager = new EnemyManager();
            _laserManager = new LaserManager();
            _backgroundManager = new BackgroundManager();

            _explosionManager = new ExplosionManager();
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
                // Use the local player's factory to create the correct laser
                GameObjectFactory factory = GetFactoryFromPlayer(_localPlayer);
                var laser = _laserManager.FireLocalLaserClient(factory, gameTime, _localPlayer.Position, _localPlayer.Rotation);

                if (laser != null)
                {
                    input.FirePressed = true;
                    var dataPacket = _localPlayer.BuildUpdatePacket();
                    PlayerFiredPacket packet = NetworkPacketFactory.Instance.MakePlayerFiredPacket(dataPacket.XPosition, dataPacket.YPosition, dataPacket.Speed, dataPacket.Rotation);
                    packet.TotalGameTime = (float)gameTime.TotalGameTime.TotalSeconds; // TOTAL GAME TIME NOT ELAPSED TIME!
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

        private GameObjectFactory GetFactoryFromPlayer(Player player)
        {
            // Use the stored element type to get the correct factory
            var elementType = _playerElements[player.NetworkID];

            switch (elementType)
            {
                case ElementalType.Fire:
                    return new FireFactory();
                case ElementalType.Electric:
                    return new ElectricFactory();
                case ElementalType.Water:
                    return new WaterFactory();
                default:
                    // Fallback to a default
                    return new FireFactory(); // Default
            }
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
                // Use the remote player's factory to create the correct laser
                Player remotePlayer = _players[playerUpdate.PlayerID];
                GameObjectFactory factory = GetFactoryFromPlayer(remotePlayer);
                _laserManager.FireRemoteLaserClient(factory, new Vector2(playerUpdate.XPosition, playerUpdate.YPosition), playerUpdate.Rotation, playerUpdate.PlayerID, playerUpdate.SendDate, playerUpdate.LaserID);
            }
        }

        private void ClientMessenger_OnEnemySpawnedPacket(BasePacket packet)
        {
            EnemySpawnedPacket enemySpawn = (EnemySpawnedPacket)packet;
            var parentEnemy = _enemyManager.AddEnemy(enemySpawn.EnemyType, new Vector2(enemySpawn.XPosition, enemySpawn.YPosition));
            parentEnemy.EnemyID = enemySpawn.EnemyID;

            // If the packet contains minion info, create them
            if (enemySpawn.Minions != null && enemySpawn.Minions.Count > 0)
            {
                foreach (var minionInfo in enemySpawn.Minions)
                {
                    // Use the prototype pattern to create minions, just like the server does.
                    var minion = parentEnemy.DeepClone();
                    minion.EnemyID = minionInfo.EnemyID;
                    minion.Scale *= 0.6f; // Make it smaller
                    minion.EnemyAnimation.SetColor(new Color(255, 255, 150)); // Give it a yellowish tint
                    parentEnemy.Minions.Add(minion);
                }
            }
        }

        private void ClientMessenger_OnEnemyEventPacket(BasePacket packet)
        {
            Console.WriteLine("--> Client received enemy event packet.");
            var enemyEvent = (EnemyEventPacket)packet;
            _enemyManager.NotifyEnemies(enemyEvent.EventType);   
        }

        private void ClientMessenger_OnEnemyDefeatedPacket(EnemyDefeatedPacket packet)
        {
            EnemyDefeatedPacket enemyDefeatedPacket = packet;

            _GUI.UpdatePlayerScore(enemyDefeatedPacket.AttackingPlayerID, enemyDefeatedPacket.AttackingPlayerNewScore);

            _laserManager.DeactivateLaser(enemyDefeatedPacket.CollidedLaserID);

            var enemy = _enemyManager.DeactivateAndReturnEnemy(enemyDefeatedPacket.CollidedEnemyID);

            // Use the attacking player's factory to create the correct explosion
            Player attackingPlayer = _players[packet.AttackingPlayerID];
            GameObjectFactory factory = GetFactoryFromPlayer(attackingPlayer);
            // Choose explosion color based on element
            Color explosionColor = Color.White;
            var element = _playerElements[attackingPlayer.NetworkID];
            switch (element)
            {
                case ElementalType.Fire:
                    explosionColor = Color.Red;
                    break;
                case ElementalType.Electric:
                    explosionColor = Color.Yellow;
                    break;
                case ElementalType.Water:
                    explosionColor = Color.Blue;
                    break;
            }
            _explosionManager.AddExplosion(enemy.Position, factory, explosionColor);
        }

        private void ClientMessenger_OnPlayerDefeatedPacket(BasePacket packet)
        {
            PlayerDefeatedPacket playerDefeatedPacket = (PlayerDefeatedPacket)packet;

            _GUI.UpdatePlayerScore(playerDefeatedPacket.CollidedPlayerID, playerDefeatedPacket.CollidedPlayerNewScore);

            _laserManager.DeactivateLaser(playerDefeatedPacket.CollidedLaserID);

            var player = _players[playerDefeatedPacket.CollidedPlayerID];

            // Use the defeated player's factory to create an explosion of their own element
            // Note: You might want the explosion to be the element of the ATTACKING player. If so, you'll need to add the AttackingPlayerID to the PlayerDefeatedPacket.
            GameObjectFactory factory = GetFactoryFromPlayer(player);
            // Choose explosion color based on element
            Color explosionColor = Color.White;
            var element = _playerElements[player.NetworkID];
            switch (element)
            {
                case ElementalType.Fire:
                    explosionColor = Color.Red;
                    break;
                case ElementalType.Electric:
                    explosionColor = Color.Yellow;
                    break;
                case ElementalType.Water:
                    explosionColor = Color.Blue;
                    break;
            }
            _explosionManager.AddExplosion(player.Position, factory, explosionColor);
        }

        private void HandleEnemyClonePacket(EnemyClonePacket packet)
        {
            Enemy originalToCopy = null;
            // Find the enemy to clone by its ID
            foreach (var enemy in _enemyManager.Enemies)
            {
                if (enemy.EnemyID == packet.EnemyID)
                {
                    originalToCopy = enemy;
                    break;
                }
            }

            if (originalToCopy != null)
            {
                Enemy copy = null;
                if (packet.IsDeepClone)
                {
                    copy = originalToCopy.DeepClone();
                    Logger.Instance.Info($"Deep copy made for EnemyID: {originalToCopy.EnemyID}. New ID: {copy.EnemyID}");
                }
                else // Shallow clone
                {
                    copy = originalToCopy.ShallowClone();
                    Logger.Instance.Info($"Shallow copy made for EnemyID: {originalToCopy.EnemyID}. New ID: {copy.EnemyID}");
                }

                copy.Position = new Vector2(originalToCopy.Position.X, originalToCopy.Position.Y + 100); // Offset to see it clearly
                _enemyManager.Enemies.Add(copy);
            }
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

                case MessageType.GI_ServerSend_EnemyEvent:
                    {
                        var enemyEventPacket = (EnemyEventPacket)recievedPacket;
                        ClientMessenger_OnEnemyEventPacket(enemyEventPacket);
                        break;
                    }

                case MessageType.GI_ServerSend_EnemyDefeated:
                    {
                        var enemyPacket = (EnemyDefeatedPacket)recievedPacket;
                        ClientMessenger_OnEnemyDefeatedPacket(enemyPacket);
                        break;
                    }

                case MessageType.GI_ServerSend_PlayerDefeated:
                    {
                        var enemyPacket = (PlayerDefeatedPacket)recievedPacket;
                        ClientMessenger_OnPlayerDefeatedPacket(enemyPacket);
                        break;
                    }

                case MessageType.GI_ServerSend_EnemyClone:
                    {
                        HandleEnemyClonePacket((EnemyClonePacket)recievedPacket);
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