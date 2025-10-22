using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MultiplayerProject.Source;

public class FirePlayer : Player
{
    private readonly PlayerColour _fireColour = new PlayerColour { R = 255, G = 0, B = 0 };

    public FirePlayer() : base()
    {
        Colour = _fireColour;
    }

    public void Initialize(ContentManager content)
    {
        // Texture size for FirePlayer: 120x70
        Width = 120;
        Height = 70;
        base.Initialize(content, Colour);
    }
}
