﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Electric Laser - Uses ElectricLaserFlyweight for shared intrinsic state
    /// Only stores extrinsic state (position, rotation, etc.)
    /// </summary>
    public class ElectricLaser : Laser
    {
        private readonly Color _color = Color.LightCyan;
        private readonly float _lengthMultiplier = 1.5f;

        public ElectricLaser() : base(ElementalType.Electric)
        {
            // Flyweight is set in base constructor if available
        }

        public ElectricLaser(string ID, string playerFiredID) : base(ID, playerFiredID, ElementalType.Electric)
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