using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Animated renderer - uses the existing animation system
    /// This preserves your current rendering approach
    /// </summary>
    public class AnimatedRenderer : IEnemyRenderer
    {
        private Texture2D _texture;
        
        public void Initialize(ContentManager content)
        {
            // Load the animated enemy texture
            _texture = content.Load<Texture2D>("mineAnimation");
        }
        
        public void Render(SpriteBatch spriteBatch, Vector2 position, Animation animation)
        {
            // Use the existing animation system - preserves current behavior
            if (animation != null)
            {
                animation.Draw(spriteBatch);
            }
        }
    }
}