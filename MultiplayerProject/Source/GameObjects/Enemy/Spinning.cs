using Microsoft.Xna.Framework;
using System;

namespace MultiplayerProject.Source
{
    class Spinning : IMoveAlgorithm
    {
        public float Health { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }

        private float _totalTime = 0f;
        private const float SPIN_RADIUS = 50f;
        private const float SPIN_SPEED = 4f;

        public void BehaveDifferently()
        {
            // Spinning behavior - enemy moves in circular pattern while going left
        }

        public void Move(ref Vector2 position, GameTime gameTime)
        {
            _totalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Move left consistently
            position.X -= 6f;
            
            // Add spinning movement
            float circleX = (float)Math.Cos(_totalTime * SPIN_SPEED) * SPIN_RADIUS;
            float circleY = (float)Math.Sin(_totalTime * SPIN_SPEED) * SPIN_RADIUS;
            
            position.Y += circleY * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}