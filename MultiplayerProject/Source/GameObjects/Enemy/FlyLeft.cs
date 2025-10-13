using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source
{
    class FlyLeft : IMoveAlgorithm
    {
        public float Health { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }

        private const float FAST_SPEED = 8f;

        public void BehaveDifferently()
        {
            // FlyLeft behavior - enemy moves quickly to the left in desperation
        }

        public void Move(ref Vector2 position, GameTime gameTime)
        {
            // Move left faster when low on health
            position.X -= FAST_SPEED;
        }
    }
}