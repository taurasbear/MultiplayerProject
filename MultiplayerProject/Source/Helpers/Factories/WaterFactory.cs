using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.GameObjects.Explosions;

namespace MultiplayerProject.Source.Helpers.Factories
{
    public class WaterFactory : GameObjectFactory
    {
        public override GameObject CreateLaser() => new WaterLaser();
        public override GameObject CreateExplosion() => new WaterExplosion();
    }
}
