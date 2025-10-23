﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    public class GameSceneGUI
    {
        private SpriteFont _font;

        private Dictionary<string, int> _playerScores;
        private Dictionary<string, string> _playerNames;
        private Dictionary<string, PlayerColour> _playerColours;

        private int _playerCount;
        private int _width;
        private int _height;
        
        // Store the local player ID
        private string _localPlayerID;

        public GameSceneGUI(int width, int height, string[] playerIDs, string[] playerNames, PlayerColour[] playerColours, string localPlayerID)
        {
            _width = width;
            _height = height;
            _localPlayerID = localPlayerID;

            _playerCount = playerIDs.Length;

            _playerScores = new Dictionary<string, int>();
            _playerNames = new Dictionary<string, string>();
            _playerColours = new Dictionary<string, PlayerColour>();

            for (int i = 0; i < _playerCount; i++)
            {
                _playerScores.Add(playerIDs[i], 0);
                _playerNames.Add(playerIDs[i], playerNames[i]);
                _playerColours.Add(playerIDs[i], playerColours[i]);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int xDistanceBetweenEach = 150;
            int textWidth = 50;

            int currentXpos = (_width / 2) - (_playerCount/2 * (xDistanceBetweenEach + textWidth)); // Calculate starting xpos

            foreach (KeyValuePair<string, string> player in _playerNames)
            {
                PlayerColour colour = _playerColours[player.Key];

                // DRAW NAME
                spriteBatch.DrawString(_font, player.Value, new Vector2(currentXpos, 0), new Color(colour.R, colour.G, colour.B));
                // DRAW SCORE
                spriteBatch.DrawString(_font, _playerScores[player.Key].ToString(), new Vector2(currentXpos + ((xDistanceBetweenEach ) / 2), textWidth/2), new Color(colour.R, colour.G, colour.B));

                currentXpos += (textWidth + xDistanceBetweenEach);
            }
        }

        public void Initalise(ContentManager content)
        {
            _font = content.Load<SpriteFont>("Font");
        }

        public void UpdatePlayerScore(string playerID, int newScore)
        {
            _playerScores[playerID] = newScore;
        }

        /// <summary>
        /// Get a player's current score
        /// </summary>
        public int GetPlayerScore(string playerID)
        {
            if (_playerScores.ContainsKey(playerID))
            {
                return _playerScores[playerID];
            }
            return 0;
        }

        /// <summary>
        /// Get the current score for the local player
        /// </summary>
        public int GetLocalPlayerScore()
        {
            if (_playerScores != null && _playerScores.ContainsKey(_localPlayerID))
            {
                return _playerScores[_localPlayerID];
            }
            return 0;
        }
    }
}
