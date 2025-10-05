using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    public class RedExplosion : Explosion
    {
        public PlayerColour Colour { get; private set; }

        public RedExplosion(PlayerColour colour) : base()
        {
            Colour = colour;
        }
    }
}