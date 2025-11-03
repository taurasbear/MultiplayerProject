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
        private Vector2 _centerPosition;
        private bool _centerSet = false;
        private const float SPIN_RADIUS = 40f;
        private const float SPIN_SPEED = 3f;

        public void BehaveDifferently()
        {
            // Spinning behavior - enemy moves in circular pattern
        }

        public void Move(ref Vector2 position, GameTime gameTime)
        {
            // Set center position on first call
            if (!_centerSet)
            {
                _centerPosition = position;
                _centerSet = true;
            }

            _totalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Calculate circular movement around the center point
            float circleX = (float)Math.Cos(_totalTime * SPIN_SPEED) * SPIN_RADIUS;
            float circleY = (float)Math.Sin(_totalTime * SPIN_SPEED) * SPIN_RADIUS;
            
            // Set position relative to center
            position.X = _centerPosition.X + circleX;
            position.Y = _centerPosition.Y + circleY;
            
            // Slowly move the center to the left so enemies don't stay in one place
            _centerPosition.X -= 1f;
        }
    }
}