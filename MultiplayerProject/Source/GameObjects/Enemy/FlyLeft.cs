using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source
{
    class FlyRight : IMoveAlgorithm
    {
        public float Health { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }

        private const float FAST_SPEED = 8f;

        public void BehaveDifferently()
        {
            // FlyRight behavior - enemy moves quickly to the right
        }

        public void Move(ref Vector2 position, GameTime gameTime)
        {
            // Move right faster 
            position.X += FAST_SPEED;
        }
    }
}