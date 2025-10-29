﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.Helpers.Factories;
using MultiplayerProject.Source.GameObjects.Enemy;


namespace MultiplayerProject.Source
{
    /*
     HOW SYNCING LASER FIRING IS GOING TO WORK:
     - On the client side, when fire is pressed, a local laser is fired immediately from the player ship. A network fire packet is sent to server and then sent to clients.
     - No laser collision checks take place on the client side.

     - On the server side, each 'Player' has a LaserManager instance. 
     - When the server recieves the fire packet from the client, it updates it by the deltaTime between sending and recieving the packet, to sync it perfectly to the client
     - All Player LaserManager's and their Laser's are updated every frame
     - On the server side there is also a CollisionManager instance, checking for collisions between any any laser and any player.
     - If there is a collision, send a player killed message back to all clients, triggering a death explosion on the client. Mark the player as dead on the server too.
         
         */
    public class GameInstance : IMessageable
    {
        public event BasePacketDelegate OnGameCompleted;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        private string _gameRoomID;

        private Dictionary<string, PlayerUpdatePacket> _playerUpdates;
        private Dictionary<string, LaserManager> _playerLasers;
        private Dictionary<string, int> _playerScores;
        private Dictionary<string, string> _playerNames;
        private Dictionary<string, Player> _players;
        private Dictionary<string, Color> _playerColours;
        private Dictionary<string, ElementalType> _playerElements;
        private Dictionary<ElementalType, GameObjectFactory> _factories;

        private CollisionManager _collisionManager;
        private Random _random;

        private EnemyManager _enemyManager;
        private TimeSpan _enemySpawnTime;
        private TimeSpan _previousEnemySpawnTime;
        private int framesSinceLastSend;
        private EnemyType[] _enemyTypes = new[] { EnemyType.Mine, EnemyType.Bird, EnemyType.Blackbird };
        private int _enemySpawnCounter = 0; // Track how many enemies have been spawned

        public GameInstance(List<ServerConnection> clients, string gameRoomID)
        {
            ComponentClients = clients;

            _gameRoomID = gameRoomID;

            _playerUpdates = new Dictionary<string, PlayerUpdatePacket>();
            _playerLasers = new Dictionary<string, LaserManager>();
            _playerScores = new Dictionary<string, int>();
            _playerNames = new Dictionary<string, string>();
            _playerColours = new Dictionary<string, Color>();
            _players = new Dictionary<string, Player>();
            _playerElements = new Dictionary<string, ElementalType>();
            _factories = new Dictionary<ElementalType, GameObjectFactory>
            {
                { ElementalType.Fire, new FireFactory() },
                { ElementalType.Water, new WaterFactory() },
                { ElementalType.Electric, new ElectricFactory() }
            };
            _random = new Random();
            _collisionManager = new CollisionManager(this);
            _enemyManager = new EnemyManager();
            _previousEnemySpawnTime = TimeSpan.Zero;
            _enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            var playerColours = GenerateRandomColours(clients.Count);

            for (int i = 0; i < ComponentClients.Count; i++)
            {
                ComponentClients[i].AddServerComponent(this);
                ComponentClients[i].SendPacketToClient(NetworkPacketFactory.Instance.MakeGameInstanceInformationPacket(ComponentClients.Count, ComponentClients, playerColours, ComponentClients[i].ID), MessageType.GI_ServerSend_LoadNewGame);

                _playerUpdates[ComponentClients[i].ID] = null;
                _playerLasers[ComponentClients[i].ID] = new LaserManager();
                _playerScores[ComponentClients[i].ID] = 0;
                _playerNames[ComponentClients[i].ID] = ComponentClients[i].Name;
                _playerColours[ComponentClients[i].ID] = playerColours[i];

                Player player = new Player();
                player.NetworkID = ComponentClients[i].ID;
                _players[ComponentClients[i].ID] = player;
            }
        }

        private GameObjectFactory GetFactoryFromPlayer(Player player)
        {
            // Use the stored element type to get the correct factory
            var elementType = _playerElements.ContainsKey(player.NetworkID) ? _playerElements[player.NetworkID] : ElementalType.Fire;
            if (_factories.ContainsKey(elementType))
                return _factories[elementType];
            return _factories[ElementalType.Fire]; // Default fallback
        }

        public void RecieveClientMessage(ServerConnection client, BasePacket recievedPacket)
        {
            switch ((MessageType)recievedPacket.MessageType)
            {
                case MessageType.GI_ClientSend_PlayerUpdate:
                    {
                        var packet = (PlayerUpdatePacket)recievedPacket;
                        packet.PlayerID = client.ID;
                        _playerUpdates[client.ID] = packet;
                        break;
                    }

                case MessageType.GI_ClientSend_PlayerFired:
                    {
                        var packet = (PlayerFiredPacket)recievedPacket;
                        packet.PlayerID = client.ID;

                        var timeDifference = (packet.SendDate - DateTime.UtcNow).TotalSeconds;

                        Console.WriteLine("---> Player I think shot!");
                        _enemyManager.NotifyEnemies(EnemyEventType.PlayerShot);
                        var enemyEventPacket = NetworkPacketFactory.Instance.MakeEnemyEventPacket(EnemyEventType.PlayerShot);
                        for (int i = 0; i < ComponentClients.Count; i++)
                        {
                            ComponentClients[i].SendPacketToClient(enemyEventPacket, MessageType.GI_ServerSend_EnemyEvent);
                        }

                        // Use the player's factory to create the correct laser type
                        Player firingPlayer = _players[client.ID];
                        GameObjectFactory factory = GetFactoryFromPlayer(firingPlayer);
                        var laser = _playerLasers[client.ID].FireLaserServer(factory, packet.TotalGameTime, (float)timeDifference, new Vector2(packet.XPosition, packet.YPosition), packet.Rotation, packet.LaserID, packet.PlayerID);
                        if (laser != null)
                        {
                            for (int i = 0; i < ComponentClients.Count; i++)
                            {
                                ComponentClients[i].SendPacketToClient(packet, MessageType.GI_ServerSend_RemotePlayerFired);
                            }
                        }
                        break;
                    }
            }
        }

        public void RemoveClient(ServerConnection client)
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            bool sendPacketThisFrame = false;

            framesSinceLastSend++;

            if (framesSinceLastSend >= Application.SERVER_UPDATE_RATE)
            {
                sendPacketThisFrame = true;
                framesSinceLastSend = 0;
            }

            ApplyPlayerInput(gameTime);

            UpdateEnemies(gameTime);

            CheckCollisions();

            if (sendPacketThisFrame)
            {
                SendPlayerStatesToClients();
            }
        }

        private void ApplyPlayerInput(GameTime gameTime)
        {
            // Apply the inputs recieved from the clients to the simulation running on the server
            foreach (KeyValuePair<string, Player> player in _players)
            {
                if (_playerUpdates[player.Key] != null)
                {
                    player.Value.ApplyInputToPlayer(_playerUpdates[player.Key].Input, (float)gameTime.ElapsedGameTime.TotalSeconds);
                    player.Value.LastSequenceNumberProcessed = _playerUpdates[player.Key].SequenceNumber;
                    player.Value.LastKeyboardMovementInput = _playerUpdates[player.Key].Input;

                    player.Value.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                }

                if (_playerLasers[player.Key] != null)
                {
                    _playerLasers[player.Key].Update(gameTime);
                }
            }
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            // Spawn a new enemy enemy every 1.5 seconds
            if (gameTime.TotalGameTime - _previousEnemySpawnTime > _enemySpawnTime)
            {
                _previousEnemySpawnTime = gameTime.TotalGameTime;

                var enemy = _enemyManager.AddEnemy();

                _enemySpawnCounter++;
                bool isDeepClone = _enemySpawnCounter % 2 == 0; // Even enemies get deep clone, odd get shallow

                if (isDeepClone)
                {
                    // Every enemy gets minions (deep clone)
                    for (int i = 0; i < 2; i++)
                    {
                        var minion = (Enemy)enemy.DeepClone(); // Clone the parent to get the same type and animation base
                        minion.EnemyID = Guid.NewGuid().ToString(); // Give it a new unique ID
                        minion.Scale *= 0.6f; // Make it smaller
                        minion.EnemyAnimation.SetColor(new Color(255, 255, 150)); // Give it a yellowish tint
                        enemy.Minions.Add(minion);
                    }
                }
                // If not deep clone, enemy remains single (no minions)

                // Every enemy, send a clone request, alternating between deep and shallow
                var clonePacket = NetworkPacketFactory.Instance.MakeEnemyClonePacket(enemy.EnemyID, isDeepClone);
                Logger.Instance.Info($"Sending {(isDeepClone ? "deep" : "shallow")} clone request for EnemyID: {enemy.EnemyID}");
                for (int i = 0; i < ComponentClients.Count; i++)
                {
                    ComponentClients[i].SendPacketToClient(clonePacket, MessageType.GI_ServerSend_EnemyClone);
                }

                // Determine the type of the enemy that was just created
                EnemyType parentType = EnemyType.Mine; // Default
                if (enemy is BirdEnemy) parentType = EnemyType.Bird;
                else if (enemy is BlackbirdEnemy) parentType = EnemyType.Blackbird;

                // Create the packet and add minion info if they exist
                var packet = NetworkPacketFactory.Instance.MakeEnemySpawnedPacket(enemy.Position.X, enemy.Position.Y, enemy.EnemyID, parentType);
                if (enemy.Minions.Count > 0)
                {
                    foreach (var minion in enemy.Minions)
                    {
                        EnemyType minionType = EnemyType.Mine; // Default
                        if (minion is BirdEnemy) minionType = EnemyType.Bird;
                        else if (minion is BlackbirdEnemy) minionType = EnemyType.Blackbird;

                        packet.Minions.Add(NetworkPacketFactory.Instance.MakeMinionInfo(
                            minion.EnemyID, minion.Position.X, minion.Position.Y, (int)minionType
                        ));
                    }
                }

                // Check if players are close to winning
                EnemyEventPacket enemyEventPacket = null;
                foreach (KeyValuePair<string, int> player in _playerScores)
                {
                    if (player.Value >= Application.SCORE_TO_WIN - 1)
                    {
                        _enemyManager.NotifyEnemies(EnemyEventType.GameCloseToFinishing);
                        enemyEventPacket = NetworkPacketFactory.Instance.MakeEnemyEventPacket(EnemyEventType.GameCloseToFinishing);
                        break;
                    }
                }

                for (int i = 0; i < ComponentClients.Count; i++) // Send the enemy spawn to all clients
                {
                    packet.TotalGameTime = (float)gameTime.TotalGameTime.TotalSeconds;

                    ComponentClients[i].SendPacketToClient(packet, MessageType.GI_ServerSend_EnemySpawn);
                }

                if (enemyEventPacket != null)
                {
                    for (int i = 0; i < ComponentClients.Count; i++)
                    {
                        ComponentClients[i].SendPacketToClient(enemyEventPacket, MessageType.GI_ServerSend_EnemyEvent);
                    }
                }

                var randomType = _enemyTypes[_random.Next(_enemyTypes.Length)];
                _enemyManager.SetEnemyType(randomType);
            }

            _enemyManager.Update(gameTime);
        }

        private void CheckCollisions()
        {
            var collisions = _collisionManager.CheckCollision(_players.Values.ToList(), _enemyManager.Enemies, GetActiveLasers());

            if (collisions.Count > 0)
            {
                for (int iCollision = 0; iCollision < collisions.Count; iCollision++)
                {
                    _playerLasers[collisions[iCollision].AttackingPlayerID].DeactivateLaser(collisions[iCollision].LaserID); // Deactivate collided laser

                    if (collisions[iCollision].CollisionType == CollisionManager.CollisionType.LaserToEnemy)
                    {
                        _enemyManager.DeactivateEnemy(collisions[iCollision].DefeatedEnemyID); // Deactivate collided enemy

                        // INCREMENT PLAYER SCORE HERE
                        _playerScores[collisions[iCollision].AttackingPlayerID]++;

                        // Create packet to send to clients
                        EnemyDefeatedPacket packet = NetworkPacketFactory.Instance.MakeEnemyDefeatedPacket(collisions[iCollision].LaserID, collisions[iCollision].DefeatedEnemyID, collisions[iCollision].AttackingPlayerID, _playerScores[collisions[iCollision].AttackingPlayerID]);
                        for (int iClient = 0; iClient < ComponentClients.Count; iClient++)
                        {
                            ComponentClients[iClient].SendPacketToClient(packet, MessageType.GI_ServerSend_EnemyDefeated);
                        }
                    }
                    else
                    {
                        // DECCREMENT PLAYER SCORE HERE
                        if (_playerScores[collisions[iCollision].DefeatedPlayerID] > 0)
                            _playerScores[collisions[iCollision].DefeatedPlayerID]--;

                        // Create packet to send to clients
                        // TODO: In future move player to a random spot on the map and send that data with this packet
                        PlayerDefeatedPacket packet = NetworkPacketFactory.Instance.MakePlayerDefeatedPacket(collisions[iCollision].LaserID, collisions[iCollision].DefeatedPlayerID, _playerScores[collisions[iCollision].DefeatedPlayerID]);
                        for (int iClient = 0; iClient < ComponentClients.Count; iClient++)
                        {
                            ComponentClients[iClient].SendPacketToClient(packet, MessageType.GI_ServerSend_PlayerDefeated);
                        }
                    }
                }

                CheckGameOver();
            }
        }

        private void SendPlayerStatesToClients()
        {
            // Send a copy of the simulation on the server to all clients
            for (int i = 0; i < ComponentClients.Count; i++)
            {
                foreach (KeyValuePair<string, Player> player in _players)
                {
                    PlayerUpdatePacket updatePacket;
                    updatePacket = player.Value.BuildUpdatePacket(); // Here we are using the servers values which makes it authorative over the clients
                    updatePacket.PlayerID = player.Key;
                    updatePacket.SequenceNumber = player.Value.LastSequenceNumberProcessed;
                    updatePacket.Input = player.Value.LastKeyboardMovementInput;

                    ComponentClients[i].SendPacketToClient(updatePacket, MessageType.GI_ServerSend_UpdateRemotePlayer);
                }
            }
        }

        private void CheckGameOver()
        {
            foreach (KeyValuePair<string, int> player in _playerScores)
            {
                if (player.Value >= Application.SCORE_TO_WIN)
                {
                    Logger.Instance.Info(_playerNames[player.Key] + " has reached the score limit");

                    int playerCount = ComponentClients.Count;
                    int[] playerScores = new int[playerCount];
                    string[] playerNames = new string[playerCount];

                    int index = 0;
                    foreach (KeyValuePair<string, int> playerScore in _playerScores)
                    {
                        playerScores[index] = playerScore.Value;
                        playerNames[index] = _playerNames[playerScore.Key];
                        index++;
                    }

                    PlayerColour[] playerColours = new PlayerColour[_playerColours.Count];
                    index = 0;
                    foreach (KeyValuePair<string, Color> playerColour in _playerColours)
                    {
                        playerColours[index] = NetworkPacketFactory.Instance.MakePlayerColour(playerColour.Value.R, playerColour.Value.G, playerColour.Value.B);
                        index++;
                    }

                    LeaderboardPacket packet = NetworkPacketFactory.Instance.MakeLeaderboardPacket(playerCount, playerNames, playerScores, playerColours);
                    for (int iClient = 0; iClient < ComponentClients.Count; iClient++)
                    {
                        ComponentClients[iClient].SendPacketToClient(packet, MessageType.GI_ServerSend_GameOver);
                    }

                    OnGameCompleted(packet);

                    return;
                }
            }
        }

        private List<Color> GenerateRandomColours(int playerCount)
        {
            var returnList = new List<Color>();
            for (int i = 0; i < playerCount && i < WaitingRoom.MAX_PEOPLE_PER_ROOM; i++)
            {
                switch (i)
                {
                    case 0:
                        returnList.Add(Color.White);
                        break;
                    case 1:
                        returnList.Add(Color.Red);
                        break;
                    case 2:
                        returnList.Add(Color.Blue);
                        break;
                    case 3:
                        returnList.Add(Color.Green);
                        break;
                    case 4:
                        returnList.Add(Color.Aqua);
                        break;
                    case 5:
                        returnList.Add(Color.Pink);
                        break;
                }
            }
            return returnList;
        }

        private List<Laser> GetActiveLasers()
        {
            List<Laser> lasers = new List<Laser>();

            foreach (KeyValuePair<string, LaserManager> laserManager in _playerLasers)
            {
                lasers.AddRange(laserManager.Value.Lasers);
            }

            return lasers;
        }
    }
}