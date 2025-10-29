using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Particle renderer - renders enemies as particle effects for special visual effects
    /// </summary>
    public class ParticleRenderer : IEnemyRenderer
    {
        private Texture2D _particleTexture;
        private Random _random;
        
        public ParticleRenderer()
        {
            _random = new Random();
        }
        
        public void Initialize(ContentManager content)
        {
            // Use a simple texture for particles (can be the laser texture or create a small dot)
            _particleTexture = content.Load<Texture2D>("laser");
        }
        
        public void Render(SpriteBatch spriteBatch, Vector2 position, Animation animation)
        {
            // Create a particle effect - multiple small sprites around the position
            if (_particleTexture != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = (float)(i * Math.PI * 2 / 8);
                    float radius = 15f + (float)Math.Sin(Environment.TickCount * 0.01f) * 5f; // Pulsing effect
                    
                    Vector2 particlePos = position + new Vector2(
                        (float)Math.Cos(angle) * radius,
                        (float)Math.Sin(angle) * radius
                    );
                    
                    Color particleColor = Color.Orange * (0.7f + 0.3f * (float)Math.Sin(Environment.TickCount * 0.02f + i));
                    
                    spriteBatch.Draw(_particleTexture, particlePos, null, particleColor, angle, 
                        Vector2.Zero, 0.3f, SpriteEffects.None, 0f);
                }
            }
        }
    }
}