﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MultiplayerProject.Source
{
    public class ElectricLaser : Laser
    {
        private readonly Color _color = Color.LightCyan;
        public float LengthMultiplier { get; private set; } = 1.5f;

        public override void Initialize(Animation baseAnimation, Vector2 position, float rotation)
        {
            base.Initialize(baseAnimation, position, rotation);
            LaserColor = _color;
            baseAnimation.SetColor(_color);
            baseAnimation.Scale *= LengthMultiplier; // long and thin
        }
    }
}