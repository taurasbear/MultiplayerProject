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

        // the width of the laser image.
        public int Width
        {
            get { return LaserAnimation.FrameWidth; }
        }

        // the height of the laser image.
        public int Height
        {
            get { return LaserAnimation.FrameHeight; }
        }

        public string PlayerFiredID { get; set; }
        public Color LaserColor;
        public string LaserID { get; set; }

        // a multiplier for the damage of the laser
        public float Damage { get; set; }

        // the speed the laser travels
        public float Speed { get; set; }

        // the distance the laser can travel
        public float Range { get; set; }

        private float distanceTraveled;

        public Laser()
        {
            LaserID = Guid.NewGuid().ToString();
            PlayerFiredID = "";
            Damage = 10f;
            Speed = 30f;
            Range = 1000f;
        }

        public Laser(string ID, string playerFiredID)
        {
            LaserID = ID;
            PlayerFiredID = playerFiredID;
            Damage = 10f;
            Speed = 30f;
            Range = 1000f;
        }

        public virtual void Initialize(Animation animation, Vector2 position, float rotation)
        {
            distanceTraveled = 0;
            LaserAnimation = animation;
            Position = position;
            Rotation = rotation;
            Active = true;
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
            LaserAnimation.Draw(spriteBatch);
        }
    }
}
