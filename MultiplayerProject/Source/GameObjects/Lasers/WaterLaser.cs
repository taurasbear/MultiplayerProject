﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Water Laser - Uses WaterLaserFlyweight for shared intrinsic state
    /// Only stores extrinsic state (position, rotation, etc.)
    /// </summary>
    public class WaterLaser : Laser
    {
        private readonly Color _color = Color.CornflowerBlue;
        private readonly float _lengthMultiplier = 0.8f;

        public WaterLaser() : base(ElementalType.Water)
        {
            // Flyweight is set in base constructor if available
        }

        public WaterLaser(string ID, string playerFiredID) : base(ID, playerFiredID, ElementalType.Water)
        {
            // Flyweight is set in base constructor if available
        }

        public override void Initialize(Animation baseAnimation, Vector2 position, float rotation)
        {
            base.Initialize(baseAnimation, position, rotation);
            
            // If flyweight wasn't initialized (server-side), set properties manually
            if (_flyweight == null)
            {
                LaserColor = _color;
                baseAnimation.SetColor(_color);
                baseAnimation.Scale *= _lengthMultiplier;
            }
        }
    }
}