using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.Helpers;
using MultiplayerProject.Source.Helpers.Factories;

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

            // Create a base animation that does NOT loop
            Animation baseAnimation = new Animation();
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
                false  // Explosions should not loop
            );

            explosion.Initialize(baseAnimation, position, color);

            // Use base class method to add to collection
            AddEntityToCollection(explosion);
        }
    }
}