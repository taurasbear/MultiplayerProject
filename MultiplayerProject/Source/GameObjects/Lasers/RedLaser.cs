
namespace MultiplayerProject.Source
{
    public class RedLaser : Laser
    {
        public PlayerColour Colour { get; private set; }

        public RedLaser(PlayerColour colour) : base()
        {
            Colour = colour;
        }

        public RedLaser(PlayerColour colour, string ID, string playerFiredID) : base(ID, playerFiredID)
        {
            Colour = colour;
        }
    }
}