﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    public class ClientApplication
    {
        public static event EmptyDelegate OnRequestToReturnToMainMenu;

        private IScene _currentScene;

        private Client _client;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _contentManager;

        private int _width;
        private int _height;

        private Keys _customLeft, _customRight, _customUp, _customDown, _customFire;
        private bool _customBindingsSet = false;

        public ClientApplication(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            _client = new Client();
            _graphicsDevice = graphicsDevice;
            _contentManager = contentManager;

            _width = graphicsDevice.Viewport.Width;
            _height = graphicsDevice.Viewport.Height;

            Client.OnServerForcedDisconnect += Client_OnDisconnectedFromServer;
            Client.OnLoadNewGame += ClientMessenger_OnLoadNewGame;
            Client.OnGameOver += ClientMessenger_OnGameOver;
            LeaderboardScene.OnReturnToWaitingRoom += LeaderboardScene_OnReturnToWaitingRoom;
        }

        public void Initalise(string hostname, int port, string playerName)
        {
            Console.WriteLine("Attempting to connect...");
            if (_client.Connect(hostname, port))
            {
                Console.WriteLine("Connected...");
                try
                {
                    _client.Run();

                    var newScene = new WaitingRoomScene(_client);
                    newScene.Initalise(_contentManager, _graphicsDevice);
                    SetNewScene(newScene);

                    _client.SendMessageToServer(NetworkPacketFactory.Instance.MakeStringPacket(playerName), MessageType.Client_SendPlayerName);
                }
                catch (NotConnectedException e)
                {
                    Console.WriteLine("Client not Connected: " + e.Message);
                }
            }
            else
            {
                Console.WriteLine("Failed to connect to: " + hostname + ":" + port);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (_currentScene == null)
                return;

            _currentScene.Update(gameTime);
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            if (_currentScene == null)
                return;

            _currentScene.ProcessInput(gameTime, inputInfo);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_currentScene == null)
                return;

            _currentScene.Draw(spriteBatch);
        }

        public void OnExiting()
        {
            // Tell the server we are disconnecting
            _client.SendMessageToServer(new BasePacket(), MessageType.Client_Disconnect);
            _client.Stop();
        }

        private void Client_OnDisconnectedFromServer()
        {
            // The server has disconnected for some reason so we shall return to the main menu
            OnRequestToReturnToMainMenu();

            Console.WriteLine("Disconnected from server");
            _client.Stop();
        }

        private void ClientMessenger_OnLoadNewGame(BasePacket packet)
        {
            GameInstanceInformation gameInstance = (GameInstanceInformation)packet;

            var newScene = new GameScene(_width, _height, gameInstance.PlayerCount, gameInstance.PlayerIDs, gameInstance.PlayerNames, gameInstance.PlayerColours, gameInstance.LocalPlayerID, _client);
            newScene.Initalise(_contentManager, _graphicsDevice);

            // Apply custom bindings if set
            if (_customBindingsSet)
            {
                newScene.BindKey(_customLeft, new GameScene.MoveLeftCommand());
                newScene.BindKey(_customRight, new GameScene.MoveRightCommand());
                newScene.BindKey(_customUp, new GameScene.MoveUpCommand());
                newScene.BindKey(_customDown, new GameScene.MoveDownCommand());
                newScene.BindKey(_customFire, new GameScene.FireCommand());
            }

            SetNewScene(newScene);
        }

        private void ClientMessenger_OnGameOver(BasePacket packet)
        {
            LeaderboardPacket leaderBoard = (LeaderboardPacket)packet;

            var scene = new LeaderboardScene(_client, _width, _height, leaderBoard);
            scene.Initalise(_contentManager, _graphicsDevice);
            SetNewScene(scene);
        }

        private void LeaderboardScene_OnReturnToWaitingRoom()
        {
            var newScene = new WaitingRoomScene(_client);
            newScene.Initalise(_contentManager, _graphicsDevice);
            SetNewScene(newScene);
            newScene.RequestWaitingRoomUpdate();
        }

        private void SetNewScene(IScene scene)
        {
            _currentScene = scene;
            _client.SetCurrentScene(scene);
        }

        public GameScene GetCurrentGameScene()
        {
            return _currentScene as GameScene;
        }

        public void SetCustomBindings(Keys left, Keys right, Keys up, Keys down, Keys fire)
        {
            _customLeft = left;
            _customRight = right;
            _customUp = up;
            _customDown = down;
            _customFire = fire;
            _customBindingsSet = true;
        }
    }
}