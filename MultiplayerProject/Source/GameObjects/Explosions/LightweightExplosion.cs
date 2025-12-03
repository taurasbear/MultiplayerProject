using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.GameObjects.Explosions;
using System;

namespace MultiplayerProject.Source.GameObjects
{
    /// <summary>
    /// Lightweight explosion that uses Flyweight pattern CORRECTLY
    /// Only stores extrinsic state (position, color, frame) - NOT the heavy Animation object
    /// </summary>
    public class LightweightExplosion : GameObject
    {
        // Extrinsic state - unique to each explosion
        public Vector2 Position { get; set; }
        public Color Color { get; set; }
        public override bool Active { get; set; }
        
        private int _currentFrame;
        private int _elapsedTime;
        private ExplosionFlyweight _flyweight;
        
        // Properties from flyweight (for external access)
        public int Width => _flyweight?.FrameWidth ?? 0;
        public int Height => _flyweight?.FrameHeight ?? 0;
        public float Damage { get; set; } = 20f;
        public float Radius { get; set; } = 1.0f;

        public LightweightExplosion()
        {
            _currentFrame = 0;
            _elapsedTime = 0;
            Active = false;
        }

        /// <summary>
        /// Initialize with shared flyweight reference
        /// </summary>
        public void Initialize(ExplosionFlyweight flyweight, Vector2 position, Color color)
        {
            _flyweight = flyweight ?? throw new ArgumentNullException(nameof(flyweight));
            Position = position;
            Color = color;
            Active = true;
            _currentFrame = 0;
            _elapsedTime = 0;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Active || _flyweight == null) return;

            _elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_elapsedTime > _flyweight.FrameTime)
            {
                _currentFrame++;

                if (_currentFrame >= _flyweight.FrameCount)
                {
                    // Animation complete
                    _currentFrame = _flyweight.FrameCount - 1;
                    Active = false;
                }

                _elapsedTime = 0;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Active || _flyweight == null) return;

            // Use flyweight to draw - no Animation object needed!
            _flyweight.Draw(spriteBatch, Position, Color, 1.0f, _currentFrame);
        }
    }
}
