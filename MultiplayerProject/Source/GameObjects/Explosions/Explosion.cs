// File: Explosion.cs
// Location: MultiplayerProject/Source/Explosion.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.Visitors;

namespace MultiplayerProject.Source
{
    public class Explosion : GameObject, IVisitable
    {
        public override bool Active { get; set; }
        public Animation ExplosionAnimation;
        protected Color ExplosionColor = Color.White;
        public float Damage { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        public Vector2 Position { get { return ExplosionAnimation?.Position ?? Vector2.Zero; } }
        public float Width { get { return ExplosionAnimation?.FrameWidth ?? 0; } }
        public float Height { get { return ExplosionAnimation?.FrameHeight ?? 0; } }

        public Explosion()
        {
            Damage = 20f;
            Radius = 1.0f;
            Duration = 1.0f;
        }

        public virtual void Initialize(Animation animation, Vector2 centerPosition, Color color)
        {
            ExplosionAnimation = animation;
            Active = true;
            ExplosionColor = color;
            ExplosionAnimation.SetColor(ExplosionColor);
            ExplosionAnimation.Position = centerPosition;
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

        public void Accept(IGameObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}