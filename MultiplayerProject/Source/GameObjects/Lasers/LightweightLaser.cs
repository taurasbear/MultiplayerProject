using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.GameObjects;
using System;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Lightweight laser that TRULY uses Flyweight pattern
    /// Only stores extrinsic state (position, rotation, ID) - NOT the heavy Animation object
    /// All rendering data comes from the shared flyweight
    /// Note: Does not implement IVisitable - this class is for benchmarking only
    /// </summary>
    public class LightweightLaser : GameObject
    {
        // Extrinsic state - unique to each laser
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public override bool Active { get; set; }
        public string LaserID { get; set; }
        public string PlayerFiredID { get; set; }
        
        private float _distanceTraveled;
        private LaserFlyweight _flyweight;

        // Delegate properties to flyweight
        public int Width => _flyweight?.Width ?? 0;
        public int Height => _flyweight?.Height ?? 0;
        public float Damage => _flyweight?.Damage ?? 10f;
        public float Speed => _flyweight?.Speed ?? 30f;
        public float Range => _flyweight?.Range ?? 1000f;
        public Color LaserColor => _flyweight?.TintColor ?? Color.White;

        public LightweightLaser()
        {
            LaserID = Guid.NewGuid().ToString();
            PlayerFiredID = "";
            _distanceTraveled = 0;
            Active = false;
        }

        public LightweightLaser(string ID, string playerFiredID)
        {
            LaserID = ID;
            PlayerFiredID = playerFiredID;
            _distanceTraveled = 0;
            Active = false;
        }

        /// <summary>
        /// Initialize with shared flyweight reference
        /// NO Animation object needed!
        /// </summary>
        public void Initialize(LaserFlyweight flyweight, Vector2 position, float rotation)
        {
            _flyweight = flyweight ?? throw new ArgumentNullException(nameof(flyweight));
            Position = position;
            Rotation = rotation;
            Active = true;
            _distanceTraveled = 0;
        }

        public override void Update(GameTime gameTime)
        {
            Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Update(float deltaTime)
        {
            if (!Active || _flyweight == null) return;

            Vector2 direction = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
            direction.Normalize();
            Position += direction * Speed * deltaTime;

            _distanceTraveled += Speed * deltaTime;

            if (_distanceTraveled > Range)
            {
                Active = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Active || _flyweight == null) return;

            // Use flyweight to draw - no Animation object needed!
            _flyweight.DrawLaser(spriteBatch, Position, Rotation);
        }

        public string GetElementalEffect()
        {
            return _flyweight?.GetElementalEffect() ?? "Standard laser";
        }
    }
}
