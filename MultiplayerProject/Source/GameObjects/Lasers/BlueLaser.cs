using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    public class BlueLaser : Laser
    {
        public PlayerColour Colour { get; private set; }

        public BlueLaser(PlayerColour colour) : base()
        {
            Colour = colour;
        }

        public BlueLaser(PlayerColour colour, string ID, string playerFiredID) : base(ID, playerFiredID)
        {
            Colour = colour;
        }
    }
}