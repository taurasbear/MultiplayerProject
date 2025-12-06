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

        // Server-side timing for explosions without animation
        private float _timeAlive = 0f;
        private Vector2 _position = Vector2.Zero;
        private bool _isServerSide = false;

        public Vector2 Position 
        { 
            get { return ExplosionAnimation?.Position ?? _position; } 
            set { _position = value; }
        }
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
            _position = centerPosition;
            _isServerSide = false;
            
            if (animation != null)
            {
                ExplosionAnimation.SetColor(ExplosionColor);
                ExplosionAnimation.Position = centerPosition;
                ExplosionAnimation.Reset();
            }
        }

        /// <summary>
        /// Initialize explosion for server-side use (no animation, timer-based)
        /// </summary>
        public virtual void InitializeServerSide(Vector2 centerPosition, Color color, float duration = 1.0f)
        {
            ExplosionAnimation = null;
            Active = true;
            ExplosionColor = color;
            _position = centerPosition;
            Duration = duration;
            _timeAlive = 0f;
            _isServerSide = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (_isServerSide)
            {
                // Server-side timer-based update
                _timeAlive += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_timeAlive >= Duration)
                {
                    Active = false;
                }
            }
            else if (ExplosionAnimation != null)
            {
                // Client-side animation-based update
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