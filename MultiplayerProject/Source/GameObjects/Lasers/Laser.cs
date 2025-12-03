// File: Laser.cs
// Location: MultiplayerProject/Source/Laser.cs

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.Visitors;

namespace MultiplayerProject.Source
{
    public class Laser : GameObject, IVisitable
    {
        public Animation LaserAnimation;
        public Vector2 Position;
        public float Rotation;
        public override bool Active { get; set; }

        protected LaserFlyweight _flyweight;
        protected ElementalType _elementalType;

        public int Width
        {
            get { return _flyweight != null ? _flyweight.Width : LaserAnimation.FrameWidth; }
        }

        public int Height
        {
            get { return _flyweight != null ? _flyweight.Height : LaserAnimation.FrameHeight; }
        }

        public string PlayerFiredID { get; set; }
        public Color LaserColor;
        public string LaserID { get; set; }

        public float Damage
        {
            get
            {
                if (_flyweight != null)
                    return _flyweight.Damage;

                switch (_elementalType)
                {
                    case ElementalType.Fire: return 15f;
                    case ElementalType.Water: return 8f;
                    case ElementalType.Electric: return 7f;
                    default: return 10f;
                }
            }
            set { }
        }

        public float Speed
        {
            get
            {
                if (_flyweight != null)
                    return _flyweight.Speed;

                switch (_elementalType)
                {
                    case ElementalType.Fire: return 30f;
                    case ElementalType.Water: return 25f;
                    case ElementalType.Electric: return 40f;
                    default: return 30f;
                }
            }
            set { }
        }

        public float Range
        {
            get
            {
                if (_flyweight != null)
                    return _flyweight.Range;

                switch (_elementalType)
                {
                    case ElementalType.Fire: return 600f;
                    case ElementalType.Water: return 800f;
                    case ElementalType.Electric: return 1200f;
                    default: return 1000f;
                }
            }
            set { }
        }

        private float distanceTraveled;

        public Laser()
        {
            LaserID = Guid.NewGuid().ToString();
            PlayerFiredID = "";
            _elementalType = ElementalType.Fire;
        }

        public Laser(string ID, string playerFiredID)
        {
            LaserID = ID;
            PlayerFiredID = playerFiredID;
            _elementalType = ElementalType.Fire;
        }

        public Laser(ElementalType elementalType)
        {
            LaserID = Guid.NewGuid().ToString();
            PlayerFiredID = "";
            _elementalType = elementalType;
            TryInitializeFlyweight(elementalType);
        }

        public Laser(string ID, string playerFiredID, ElementalType elementalType)
        {
            LaserID = ID;
            PlayerFiredID = playerFiredID;
            _elementalType = elementalType;
            TryInitializeFlyweight(elementalType);
        }

        private void TryInitializeFlyweight(ElementalType elementalType)
        {
            try
            {
                _flyweight = LaserFlyweightFactory.Instance.GetFlyweight(elementalType);
            }
            catch (InvalidOperationException)
            {
                _flyweight = null;
            }
        }

        public virtual void Initialize(Animation animation, Vector2 position, float rotation)
        {
            distanceTraveled = 0;
            LaserAnimation = animation;
            Position = position;
            Rotation = rotation;
            Active = true;

            if (_flyweight != null)
            {
                _flyweight.InitializeAnimation(animation);
                LaserColor = _flyweight.TintColor;
            }
        }

        public override void Update(GameTime gameTime)
        {
            Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            LaserAnimation.Position = Position;
            LaserAnimation.Rotation = Rotation;
            LaserAnimation.Update(gameTime);
        }

        public void Update(float deltaTime)
        {
            Vector2 direction = new Vector2((float)Math.Cos(Rotation),
                                     (float)Math.Sin(Rotation));
            direction.Normalize();
            Position += direction * Speed;

            distanceTraveled += Speed;

            if (distanceTraveled > Range)
            {
                Active = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_flyweight != null)
            {
                _flyweight.Draw(spriteBatch, LaserAnimation);
            }
            else
            {
                LaserAnimation.Draw(spriteBatch);
            }
        }

        public string GetElementalEffect()
        {
            return _flyweight?.GetElementalEffect() ?? "Standard laser";
        }

        public void Accept(IGameObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}