using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Bridge interface for enemy rendering implementations
    /// </summary>
    public interface IEnemyRenderer
    {
        /// <summary>
        /// Render the enemy using this specific rendering technique
        /// </summary>
        void Render(SpriteBatch spriteBatch, Vector2 position, Animation animation);
        
        /// <summary>
        /// Initialize the renderer with content manager
        /// </summary>
        void Initialize(ContentManager content);
    }
}