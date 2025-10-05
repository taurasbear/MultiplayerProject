using MultiplayerProject.Source.GameObjects;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source.Helpers.Factories
{
    public class BlueFactory : GameObjectFactory
    {
        public override GameObject GetPlayer()
        {
            return new BluePlayer(new PlayerColour { R = 0, G = 0, B = 255 });
        }

        public override GameObject GetLaser()
        {
            return new BlueLaser(new PlayerColour { R = 0, G = 0, B = 255 });
        }

        public override GameObject GetExplosion()
        {
            return new BlueExplosion(new PlayerColour { R = 0, G = 0, B = 255 });
        }
    }
}