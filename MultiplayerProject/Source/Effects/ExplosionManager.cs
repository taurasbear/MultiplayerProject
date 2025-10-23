using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.Helpers.Factories;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    class ExplosionManager
    {
        // Collections of explosions
        private List<Explosion> _explosions;

        //Texture to hold explosion animation.
        private Texture2D _explosionTexture;

        public void Initalise(ContentManager content)
        {
            // init our collection of explosions.
            _explosions = new List<Explosion>();

            // load the explosion sheet
            _explosionTexture = content.Load<Texture2D>("explosion");
        }

        public void Update(GameTime gameTime)
        {
            for (var e = 0; e < _explosions.Count; e++)
            {
                _explosions[e].Update(gameTime);

                if (!_explosions[e].Active)
                    _explosions.Remove(_explosions[e]);
            }
        }

        public void AddExplosion(Vector2 position, GameObjectFactory factory, Color color)
        {
            // Get the correct explosion type from the factory
            Explosion explosion = (Explosion)factory.CreateExplosion();

            // Create a base animation that does NOT loop
            Animation baseAnimation = new Animation();
            baseAnimation.Initialize(_explosionTexture,
                position,
                0,
                134,
                134,
                12,
                30,
                color, // Pass the color to the animation
                1.0f,
                false); // <-- This is the fix! Explosions should not loop.

            _explosions.Add(explosion);
            explosion.Initialize(baseAnimation, position, color);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draw explosions
            for(int i = 0; i < _explosions.Count; i++)
            {
                _explosions[i].Draw(spriteBatch);
            }
        }
    }
}
