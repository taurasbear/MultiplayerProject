using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.GameObjects;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Flyweight Factory - Manages and provides shared laser flyweight instances
    /// Ensures only one instance of each concrete flyweight exists in memory
    /// This is the key to the Flyweight pattern - sharing intrinsic state
    /// </summary>
    public class LaserFlyweightFactory
    {
        private static LaserFlyweightFactory _instance;
        private Dictionary<ElementalType, LaserFlyweight> _flyweights;
        private bool _initialized = false;

        private LaserFlyweightFactory()
        {
            _flyweights = new Dictionary<ElementalType, LaserFlyweight>();
        }

        public static LaserFlyweightFactory Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LaserFlyweightFactory();
                return _instance;
            }
        }

        /// <summary>
        /// Initialize concrete flyweights for each elemental type
        /// Call this once during game initialization
        /// </summary>
        public void Initialize(ContentManager content)
        {
            if (_initialized)
                return;

            Texture2D laserTexture = content.Load<Texture2D>("laser");

            // Get texture dimensions
            int width = 46;
            int height = 16;

            // Create concrete flyweight instances - only 3 instances for all lasers!
            _flyweights[ElementalType.Fire] = new FireLaserFlyweight(laserTexture, width, height);
            _flyweights[ElementalType.Water] = new WaterLaserFlyweight(laserTexture, width, height);
            _flyweights[ElementalType.Electric] = new ElectricLaserFlyweight(laserTexture, width, height);

            _initialized = true;
        }

        /// <summary>
        /// Get the shared concrete flyweight for a specific elemental type
        /// Returns the same instance for all lasers of that type
        /// </summary>
        public LaserFlyweight GetFlyweight(ElementalType type)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("LaserFlyweightFactory must be initialized before use!");
            }

            if (_flyweights.ContainsKey(type))
            {
                return _flyweights[type];
            }

            // Fallback to fire if type not found
            return _flyweights[ElementalType.Fire];
        }

        /// <summary>
        /// Get statistics about memory usage (for educational purposes)
        /// </summary>
        public string GetMemoryStats()
        {
            return $"LaserFlyweight: {_flyweights.Count} concrete flyweight instances in memory (shared across all lasers)";
        }

        /// <summary>
        /// Educational: Show which concrete flyweight types are loaded
        /// </summary>
        public List<string> GetLoadedFlyweightTypes()
        {
            List<string> types = new List<string>();
            foreach (var kvp in _flyweights)
            {
                types.Add($"{kvp.Key}: {kvp.Value.GetType().Name}");
            }
            return types;
        }
    }
}
