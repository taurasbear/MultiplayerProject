using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    public class BlueExplosion : Explosion
    {
        public PlayerColour Colour { get; private set; }

        public BlueExplosion(PlayerColour colour) : base()
        {
            Colour = colour;
        }
    }
}