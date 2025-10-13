using Microsoft.Xna.Framework;
using System;

namespace MultiplayerProject.Source
{
    class VerticalPacing : IMoveAlgorithm
    {
        public float Health { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }

        private float _totalTime = 0f;
        private const float PACING_RANGE = 100f;
        private const float PACING_SPEED = 2f;

        public void BehaveDifferently()
        {
            // Vertical pacing behavior - enemy moves up and down while moving left
        }

        public void Move(ref Vector2 position, GameTime gameTime)
        {
            _totalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Move left consistently
            position.X -= 6f;
            
            // Add vertical pacing movement
            float verticalOffset = (float)Math.Sin(_totalTime * PACING_SPEED) * PACING_RANGE;
            position.Y += verticalOffset * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}