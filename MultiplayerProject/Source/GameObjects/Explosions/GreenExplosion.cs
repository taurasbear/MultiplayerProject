using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    public class GreenExplosion : Explosion
    {
        public PlayerColour Colour { get; private set; }

        public GreenExplosion(PlayerColour colour) : base()
        {
            Colour = colour;
        }
    }
}