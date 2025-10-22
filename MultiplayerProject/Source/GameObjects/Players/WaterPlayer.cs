using Microsoft.Xna.Framework.Content;
using MultiplayerProject.Source;

public class WaterPlayer : Player
{
    private readonly PlayerColour _waterColour = new PlayerColour { R = 0, G = 0, B = 255 }; // Blue

    public WaterPlayer() : base()
    {
        Colour = _waterColour;
    }

    public void Initialize(ContentManager content)
    {
        // Texture size for WaterPlayer: 115x65
        Width = 115;
        Height = 65;
        base.Initialize(content, Colour);
    }
}