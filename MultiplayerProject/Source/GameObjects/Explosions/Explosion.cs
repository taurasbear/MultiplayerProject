using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.GameObjects;

namespace MultiplayerProject.Source
{
    public class Explosion : GameObject
    {
        public int Width
        {
            get { return _explosionAnimation.FrameWidth; }
        }

        public int Height
        {
            get { return _explosionAnimation.FrameWidth; }
        }

        public override bool Active { get; set; }

        private Animation _explosionAnimation;
        private Vector2 _position;      
        private int _timeToLive;

        public void Initialize(Animation animation, Vector2 position)
        {
            _explosionAnimation = animation;
            _position = position;
            _timeToLive = 30;

            Active = true;
        }

        public override void Update(GameTime gameTime)
        {
            _explosionAnimation.Update(gameTime);

            _timeToLive -= 1;

            if (_timeToLive <= 0)
            {
                Active = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _explosionAnimation.Draw(spriteBatch);
        }
    }
}
