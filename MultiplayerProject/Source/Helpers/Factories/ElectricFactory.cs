using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.GameObjects.Explosions;

namespace MultiplayerProject.Source.Helpers.Factories
{
    public class ElectricFactory : GameObjectFactory
    {
        public override GameObject GetLaser() => new ElectricLaser();
        public override GameObject GetExplosion() => new ElectricExplosion();
    }
}
