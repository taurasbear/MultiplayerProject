using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.GameObjects;

namespace MultiplayerProject.Source
{
    public class Explosion : GameObject
    {
        public override bool Active { get; set; }
        protected Animation ExplosionAnimation;
        protected Color ExplosionColor = Color.White; // Default

        public virtual void Initialize(Animation animation, Vector2 centerPosition, Color color)
        {
            ExplosionAnimation = animation;
            Active = true;
            ExplosionColor = color;
            ExplosionAnimation.SetColor(ExplosionColor);

            // Center animation
            ExplosionAnimation.Position = centerPosition;

            // Reset animation so it starts from first frame
            ExplosionAnimation.Reset();
        }

        public override void Update(GameTime gameTime)
        {
            if (ExplosionAnimation != null)
            {
                ExplosionAnimation.Update(gameTime);
                if (ExplosionAnimation.IsFinished)
                {
                    Active = false;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active && ExplosionAnimation != null)
            {
                ExplosionAnimation.Draw(spriteBatch);
            }
        }
    }
}
