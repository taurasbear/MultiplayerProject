using Microsoft.Xna.Framework;
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
                int playerScore = _playerScores[player.Key];

                // DRAW NAME
                spriteBatch.DrawString(_font, player.Value, new Vector2(currentXpos, 0), new Color(colour.R, colour.G, colour.B));
                // DRAW SCORE
                spriteBatch.DrawString(_font, playerScore.ToString(), new Vector2(currentXpos + ((xDistanceBetweenEach ) / 2), textWidth/2), new Color(colour.R, colour.G, colour.B));
                
                // DRAW SHIELD STATUS
                if (playerScore >= 5) // Shield threshold
                {
                    spriteBatch.DrawString(_font, "SHIELD", new Vector2(currentXpos, textWidth), Color.Cyan);
                }

                currentXpos += (textWidth + xDistanceBetweenEach);
            }
            
            // Draw fire rate indicator for local player
            if (!string.IsNullOrEmpty(_localPlayerID) && _playerScores.ContainsKey(_localPlayerID))
            {
                int localScore = _playerScores[_localPlayerID];
                float fireRateMultiplier = 1.0f + (localScore / 3) * 0.25f;
                fireRateMultiplier = Math.Min(fireRateMultiplier, 3.0f);
                
                string fireRateText = $"Fire Rate: {fireRateMultiplier:F1}x";
                spriteBatch.DrawString(_font, fireRateText, new Vector2(10, _height - 50), Color.Yellow);
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
