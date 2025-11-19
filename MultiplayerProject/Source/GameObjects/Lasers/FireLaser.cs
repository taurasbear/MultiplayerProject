﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Fire Laser - Uses FireLaserFlyweight for shared intrinsic state
    /// Only stores extrinsic state (position, rotation, etc.)
    /// </summary>
    public class FireLaser : Laser
    {
        private readonly Color _color = Color.OrangeRed;
        private readonly float _lengthMultiplier = 1.2f;

        public FireLaser() : base(ElementalType.Fire)
        {
            // Flyweight is set in base constructor if available
        }

        public FireLaser(string ID, string playerFiredID) : base(ID, playerFiredID, ElementalType.Fire)
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