using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Concrete Flyweight for Fire lasers
    /// Contains fire-specific intrinsic state (shared data)
    /// </summary>
    public class FireLaserFlyweight : LaserFlyweight
    {
        public FireLaserFlyweight(Texture2D texture, int width, int height)
        {
            Texture = texture;
            TintColor = Color.OrangeRed;
            Width = width;
            Height = height;
            Damage = 15f;
            Speed = 30f;
            Range = 600f;
            LengthMultiplier = 1.2f;
        }

        public override string GetElementalEffect()
        {
            return "Burning damage over time";
        }

        public override void Draw(SpriteBatch spriteBatch, Animation animation)
        {
            // Fire lasers could have special glow effect
            base.Draw(spriteBatch, animation);
            
            // Optional: Add fire particle effects here in future
        }
    }
}
