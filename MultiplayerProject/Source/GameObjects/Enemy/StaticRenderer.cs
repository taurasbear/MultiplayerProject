using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Static renderer - draws enemies as simple static sprites for better performance
    /// </summary>
    public class StaticRenderer : IEnemyRenderer
    {
        private Texture2D _texture;
        
        public void Initialize(ContentManager content)
        {
            // Load a static enemy texture (you can use the same texture or a different one)
            _texture = content.Load<Texture2D>("player"); // Using existing texture as placeholder
        }
        
        public void Render(SpriteBatch spriteBatch, Vector2 position, Animation animation)
        {
            // Draw as a simple static sprite - better performance
            if (_texture != null)
            {
                spriteBatch.Draw(_texture, position, Color.Red); // Red tint for enemies
            }
        }
    }
}