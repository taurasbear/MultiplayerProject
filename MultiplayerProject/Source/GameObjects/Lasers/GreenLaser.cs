using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    public class GreenLaser : Laser
    {
        public PlayerColour Colour { get; private set; }

        public GreenLaser(PlayerColour colour) : base()
        {
            Colour = colour;
        }

        public GreenLaser(PlayerColour colour, string ID, string playerFiredID) : base(ID, playerFiredID)
        {
            Colour = colour;
        }
    }
}