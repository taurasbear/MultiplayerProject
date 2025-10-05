using MultiplayerProject.Source.GameObjects;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source.Helpers.Factories
{
    public class GreenFactory : GameObjectFactory
    {
        public override GameObject GetPlayer()
        {
            return new GreenPlayer(new PlayerColour { R = 0, G = 255, B = 0 });
        }

        public override GameObject GetLaser()
        {
            return new GreenLaser(new PlayerColour { R = 0, G = 255, B = 0 });
        }

        public override GameObject GetExplosion()
        {
            return new GreenExplosion(new PlayerColour { R = 0, G = 255, B = 0 });
        }
    }
}