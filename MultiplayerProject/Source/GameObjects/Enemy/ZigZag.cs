using Microsoft.Xna.Framework;
using System;

namespace MultiplayerProject.Source
{
    class ZigZag : IMoveAlgorithm
    {
        public float Health { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }

        private float _totalTime = 0f;
        private const float ZIGZAG_FREQUENCY = 3f;
        private const float ZIGZAG_AMPLITUDE = 150f;

        public void BehaveDifferently()
        {
            // ZigZag behavior - enemy moves in a zigzag pattern
        }

        public void Move(ref Vector2 position, GameTime gameTime)
        {
            _totalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Move left consistently
            position.X -= 6f;
            
            // Add zigzag movement
            float zigzagOffset = (float)Math.Sin(_totalTime * ZIGZAG_FREQUENCY) * ZIGZAG_AMPLITUDE;
            position.Y += zigzagOffset * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}