using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MultiplayerProject.Source;

public class ElectricPlayer : Player
{
    private readonly PlayerColour _electricColour = new PlayerColour { R = 255, G = 255, B = 0 }; // Yellow

    public ElectricPlayer() : base()
    {
        Colour = _electricColour;
    }

    public void Initialize(ContentManager content)
    {
        // Texture size for ElectricPlayer: 118x68
        Width = 118;
        Height = 68;
        base.Initialize(content, Colour);
    }
}