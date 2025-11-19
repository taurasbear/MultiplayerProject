using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Concrete Flyweight for Water lasers
    /// Contains water-specific intrinsic state (shared data)
    /// </summary>
    public class WaterLaserFlyweight : LaserFlyweight
    {
        public WaterLaserFlyweight(Texture2D texture, int width, int height)
        {
            Texture = texture;
            TintColor = Color.CornflowerBlue;
            Width = width;
            Height = height;
            Damage = 8f;
            Speed = 25f;
            Range = 800f;
            LengthMultiplier = 0.8f;
        }

        public override string GetElementalEffect()
        {
            return "Slowing effect on hit";
        }

        public override void Draw(SpriteBatch spriteBatch, Animation animation)
        {
            // Water lasers could have wave effect
            base.Draw(spriteBatch, animation);
            
            // Optional: Add water splash effects here in future
        }
    }
}
