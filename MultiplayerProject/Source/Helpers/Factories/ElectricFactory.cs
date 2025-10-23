using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.GameObjects.Explosions;

namespace MultiplayerProject.Source.Helpers.Factories
{
    public class ElectricFactory : GameObjectFactory
    {
        public override GameObject CreateLaser() => new ElectricLaser();
        public override GameObject CreateExplosion() => new ElectricExplosion();
    }
}
