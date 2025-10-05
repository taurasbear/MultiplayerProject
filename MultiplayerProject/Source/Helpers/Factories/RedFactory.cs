using MultiplayerProject.Source.GameObjects;

namespace MultiplayerProject.Source.Helpers.Factories
{
    public class RedFactory : GameObjectFactory
    {
        public override GameObject GetPlayer()
        {
            return new RedPlayer(new PlayerColour { R = 255, G = 0, B = 0 });
        }

        public override GameObject GetLaser()
        {
            return new RedLaser(new PlayerColour { R = 255, G = 0, B = 0 });
        }

        public override GameObject GetExplosion()
        {
            return new RedExplosion(new PlayerColour { R = 255, G = 0, B = 0 });
        }
    }
}