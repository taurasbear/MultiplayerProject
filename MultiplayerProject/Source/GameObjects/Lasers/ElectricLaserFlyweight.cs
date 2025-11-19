using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Concrete Flyweight for Electric lasers
    /// Contains electric-specific intrinsic state (shared data)
    /// </summary>
    public class ElectricLaserFlyweight : LaserFlyweight
    {
        public ElectricLaserFlyweight(Texture2D texture, int width, int height)
        {
            Texture = texture;
            TintColor = Color.LightCyan;
            Width = width;
            Height = height;
            Damage = 7f;
            Speed = 40f;
            Range = 1200f;
            LengthMultiplier = 1.5f;
        }

        public override string GetElementalEffect()
        {
            return "Chain lightning to nearby enemies";
        }

        public override void Draw(SpriteBatch spriteBatch, Animation animation)
        {
            // Electric lasers could have lightning effect
            base.Draw(spriteBatch, animation);
            
            // Optional: Add electric spark effects here in future
        }
    }
}
