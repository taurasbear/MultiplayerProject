using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MultiplayerProject.Source.GameObjects.Explosions
{
    /// <summary>
    /// Singleton factory that manages the single shared ExplosionFlyweight instance
    /// This ensures only ONE flyweight exists in memory for ALL explosions
    /// </summary>
    public class ExplosionFlyweightFactory
    {
        private static ExplosionFlyweightFactory _instance;
        private ExplosionFlyweight _explosionFlyweight;
        private bool _initialized = false;

        private ExplosionFlyweightFactory() { }

        public static ExplosionFlyweightFactory Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ExplosionFlyweightFactory();
                return _instance;
            }
        }

        /// <summary>
        /// Initialize the flyweight with explosion texture data
        /// Call this once during game initialization
        /// </summary>
        public void Initialize(ContentManager content)
        {
            if (_initialized)
                return;

            Texture2D explosionTexture = content.Load<Texture2D>("explosion");

            // Create the single shared flyweight instance for ALL explosions
            _explosionFlyweight = new ExplosionFlyweight(
                texture: explosionTexture,
                frameWidth: 134,
                frameHeight: 134,
                frameCount: 12,
                frameTime: 30
            );

            _initialized = true;
        }

        /// <summary>
        /// Get the shared flyweight instance
        /// Returns the same instance for ALL explosions
        /// </summary>
        public ExplosionFlyweight GetFlyweight()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("ExplosionFlyweightFactory must be initialized before use!");
            }

            return _explosionFlyweight;
        }

        /// <summary>
        /// Check if the factory has been initialized
        /// </summary>
        public bool IsInitialized => _initialized;
    }
}
