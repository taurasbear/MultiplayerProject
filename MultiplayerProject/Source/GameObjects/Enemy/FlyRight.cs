using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source
{
    class FlyRight : IMoveAlgorithm
    {
        public float Health { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }

        private const float FAST_SPEED = 8f;
        private const float DIRECTION_CHANGE_TIME = 2f; // Change direction every 2 seconds
        private float _timeInCurrentDirection = 0f;
        private bool _movingRight = true;

        public void BehaveDifferently()
        {
            // FlyRight behavior - enemy moves right then left alternately
        }

        public void Move(ref Vector2 position, GameTime gameTime)
        {
            _timeInCurrentDirection += (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Change direction every DIRECTION_CHANGE_TIME seconds
            if (_timeInCurrentDirection >= DIRECTION_CHANGE_TIME)
            {
                _movingRight = !_movingRight;
                _timeInCurrentDirection = 0f;
            }
            
            // Move right or left based on current direction
            if (_movingRight)
            {
                position.X += FAST_SPEED;
            }
            else
            {
                position.X -= FAST_SPEED;
            }
        }
    }
}