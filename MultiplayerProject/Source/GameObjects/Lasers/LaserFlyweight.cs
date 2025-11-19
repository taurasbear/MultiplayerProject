using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Abstract Flyweight - Defines interface for flyweight objects
    /// Contains shared (intrinsic) laser data that is common across all lasers of the same type
    /// </summary>
    public abstract class LaserFlyweight
    {
        // Intrinsic state - shared among all lasers of this type
        public Texture2D Texture { get; protected set; }
        public Color TintColor { get; protected set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public float Damage { get; protected set; }
        public float Speed { get; protected set; }
        public float Range { get; protected set; }
        public float LengthMultiplier { get; protected set; }

        /// <summary>
        /// Draw the laser using extrinsic state (position, rotation, animation)
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch, Animation animation)
        {
            animation.Draw(spriteBatch);
        }

        /// <summary>
        /// Get elemental-specific effects or properties
        /// </summary>
        public abstract string GetElementalEffect();

        /// <summary>
        /// Initialize animation with flyweight's intrinsic properties
        /// </summary>
        public virtual void InitializeAnimation(Animation animation)
        {
            animation.SetColor(TintColor);
            animation.Scale *= LengthMultiplier;
        }
    }
}
