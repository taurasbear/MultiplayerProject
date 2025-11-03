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
        public void Initialize(ContentManager content)
        {
            // No need to load any texture here - we use the animation already set on the enemy
            // Each enemy type (BirdEnemy, BlackbirdEnemy) has its own animation with correct texture
        }
        
        public void Render(SpriteBatch spriteBatch, Vector2 position, Animation animation)
        {
            // Use the existing animation system - just draw the animation that was set on the enemy
            if (animation != null)
            {
                animation.Draw(spriteBatch);
            }
        }
    }
}