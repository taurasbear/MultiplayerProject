﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MultiplayerProject.Source
{
    public class FireLaser : Laser
    {
        private readonly Color _color = Color.OrangeRed;

        public float LengthMultiplier { get; private set; } = 1.2f;

        public FireLaser()
        {
            Damage = 15f;
            Speed = 30f;
            Range = 600f;
        }

        public override void Initialize(Animation baseAnimation, Vector2 position, float rotation)
        {
            base.Initialize(baseAnimation, position, rotation);
            LaserColor = _color;
            baseAnimation.SetColor(_color);
            baseAnimation.Scale *= LengthMultiplier; // stretch the laser
        }
    }
}