﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.GameObjects;

namespace MultiplayerProject.Source
{
    public class Laser : GameObject
    {
        // animation the represents the laser animation.
        public Animation LaserAnimation;

        // position of the laser
        public Vector2 Position;

        public float Rotation;

        // set the laser to active
        public override bool Active { get; set; }

        // Reference to shared flyweight (intrinsic state)
        protected LaserFlyweight _flyweight;

        // Elemental type for this laser instance
        protected ElementalType _elementalType;

        // the width of the laser image (from flyweight)
        public int Width
        {
            get { return _flyweight != null ? _flyweight.Width : LaserAnimation.FrameWidth; }
        }

        // the height of the laser image (from flyweight)
        public int Height
        {
            get { return _flyweight != null ? _flyweight.Height : LaserAnimation.FrameHeight; }
        }

        public string PlayerFiredID { get; set; }
        public Color LaserColor;
        public string LaserID { get; set; }

        // a multiplier for the damage of the laser (from flyweight or fallback)
        public float Damage 
        { 
            get 
            { 
                if (_flyweight != null) 
                    return _flyweight.Damage;
                
                // Fallback values for server-side (no flyweight)
                switch (_elementalType)
                {
                    case ElementalType.Fire: return 15f;
                    case ElementalType.Water: return 8f;
                    case ElementalType.Electric: return 7f;
                    default: return 10f;
                }
            }
            set { } // Keep setter for backward compatibility
        }

        // the speed the laser travels (from flyweight or fallback)
        public float Speed 
        { 
            get 
            { 
                if (_flyweight != null) 
                    return _flyweight.Speed;
                
                // Fallback values for server-side (no flyweight)
                switch (_elementalType)
                {
                    case ElementalType.Fire: return 30f;
                    case ElementalType.Water: return 25f;
                    case ElementalType.Electric: return 40f;
                    default: return 30f;
                }
            }
            set { } // Keep setter for backward compatibility
        }

        // the distance the laser can travel (from flyweight or fallback)
        public float Range 
        { 
            get 
            { 
                if (_flyweight != null) 
                    return _flyweight.Range;
                
                // Fallback values for server-side (no flyweight)
                switch (_elementalType)
                {
                    case ElementalType.Fire: return 600f;
                    case ElementalType.Water: return 800f;
                    case ElementalType.Electric: return 1200f;
                    default: return 1000f;
                }
            }
            set { } // Keep setter for backward compatibility
        }

        private float distanceTraveled;

        public Laser()
        {
            LaserID = Guid.NewGuid().ToString();
            PlayerFiredID = "";
            _elementalType = ElementalType.Fire; // Default
        }

        public Laser(string ID, string playerFiredID)
        {
            LaserID = ID;
            PlayerFiredID = playerFiredID;
            _elementalType = ElementalType.Fire; // Default
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
                // Flyweight factory not initialized (server-side), flyweight will remain null
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

            // If flyweight is set, use it to initialize animation properties
            if (_flyweight != null)
            {
                _flyweight.InitializeAnimation(animation);
                LaserColor = _flyweight.TintColor;
            }
            // Otherwise, subclasses will set their own properties directly
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
            // Use flyweight to draw if available
            if (_flyweight != null)
            {
                _flyweight.Draw(spriteBatch, LaserAnimation);
            }
            else
            {
                LaserAnimation.Draw(spriteBatch);
            }
        }

        /// <summary>
        /// Get the elemental effect description from flyweight
        /// </summary>
        public string GetElementalEffect()
        {
            return _flyweight?.GetElementalEffect() ?? "Standard laser";
        }
    }
}
