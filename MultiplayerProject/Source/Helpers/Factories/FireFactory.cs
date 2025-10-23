using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.GameObjects.Explosions;

namespace MultiplayerProject.Source.Helpers.Factories
{
    public class FireFactory : GameObjectFactory
    {
        public override GameObject CreateLaser() => new FireLaser();
        public override GameObject CreateExplosion() => new FireExplosion();
    }
}
