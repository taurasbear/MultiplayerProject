using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.Helpers;
using MultiplayerProject.Source.Helpers.Factories;
using MultiplayerProject.Source.GameObjects;

namespace MultiplayerProject.Source
{
    public sealed class ExplosionManager : EntityManagerBase<Explosion>
    {
        // Explosion-specific texture
        private Texture2D _explosionTexture;

        public ExplosionManager() : base()
        {
        }

        /// <summary>
        /// Implements abstract method: Initalise explosion-specific resources.
        /// </summary>
        public override void Initalise(ContentManager content)
        {
            _explosionTexture = content.Load<Texture2D>("explosion");
            
            // Initialize Flyweight factory for memory optimization
            GameObjects.Explosions.ExplosionFlyweightFactory.Instance.Initialize(content);
        }

        /// <summary>
        /// Implements abstract method: Update single explosion entity.
        /// </summary>
        protected override void UpdateEntity(Explosion explosion, GameTime gameTime)
        {
            explosion.Update(gameTime);
        }

        /// <summary>
        /// Implements abstract method: Explosion removal criteria.
        /// </summary>
        protected override bool ShouldRemoveEntity(Explosion explosion)
        {
            // Remove if explosion animation is complete
            return !explosion.Active;
        }

        /// <summary>
        /// Implements abstract method: Draw single explosion entity.
        /// </summary>
        protected override void DrawEntity(Explosion explosion, SpriteBatch spriteBatch)
        {
            explosion.Draw(spriteBatch);
        }

        public void AddExplosion(Vector2 position, GameObjectFactory factory, Color color)
        {
            // Get the correct explosion type from the factory
            Explosion explosion = (Explosion)factory.CreateExplosion();

            // Use Flyweight pattern to create animation with shared intrinsic state
            // This saves memory by reusing the same texture and properties for all explosions
            Animation baseAnimation;
            
            if (GameObjects.Explosions.ExplosionFlyweightFactory.Instance.IsInitialized)
            {
                var flyweight = GameObjects.Explosions.ExplosionFlyweightFactory.Instance.GetFlyweight();
                baseAnimation = flyweight.CreateAnimation(position, color, 1.0f, false);
            }
            else
            {
                // Fallback if flyweight not initialized (shouldn't happen)
                baseAnimation = new Animation();
                baseAnimation.Initialize(
                    _explosionTexture,
                    position,
                    0,
                    134,
                    134,
                    12,
                    30,
                    color,
                    1.0f,
                    false
                );
            }

            explosion.Initialize(baseAnimation, position, color);

            // Use base class method to add to collection
            AddEntityToCollection(explosion);
        }

        /// <summary>
        /// Add a lightweight explosion using TRUE flyweight pattern (no Animation objects)
        /// This creates explosions that only store position, color, and current frame
        /// All rendering data comes from the shared flyweight
        /// </summary>
        public LightweightExplosion AddLightweightExplosion(Vector2 position, Color color)
        {
            // Get the shared flyweight
            var flyweight = GameObjects.Explosions.ExplosionFlyweightFactory.Instance.GetFlyweight();

            // Create lightweight explosion - only stores position, color, frame
            LightweightExplosion explosion = new LightweightExplosion();
            explosion.Initialize(flyweight, position, color);

            // Note: LightweightExplosion is not added to the main collection
            // as EntityManagerBase<Explosion> expects Explosion type.
            // This method is only used for benchmarking purposes.

            return explosion;
        }

        /// <summary>
        /// Server-side explosion creation without graphics (for statistics tracking only)
        /// This allows the server to track explosions in the visitor pattern without needing textures
        /// </summary>
        public void AddServerExplosion(Vector2 position, GameObjectFactory factory, Color color)
        {
            // Get the correct explosion type from the factory
            Explosion explosion = (Explosion)factory.CreateExplosion();

            // Initialize for server-side use (no graphics, timer-based)
            explosion.InitializeServerSide(position, color, 1.0f); // 1 second duration
            
            // Use base class method to add to collection
            AddEntityToCollection(explosion);
        }
    }
}