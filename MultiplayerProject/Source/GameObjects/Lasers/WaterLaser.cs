﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MultiplayerProject.Source
{
    public class WaterLaser : Laser
    {
        private readonly Color _color = Color.CornflowerBlue;
        public float LengthMultiplier { get; private set; } = 0.8f;

        public WaterLaser()
        {
            Damage = 8f;
            Speed = 25f;
            Range = 800f;
        }

        public override void Initialize(Animation baseAnimation, Vector2 position, float rotation)
        {
            base.Initialize(baseAnimation, position, rotation);
            LaserColor = _color;
            baseAnimation.SetColor(_color);
            baseAnimation.Scale *= LengthMultiplier; // shrink slightly
        }
    }
}