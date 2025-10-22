using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.GameObjects.Explosions;

namespace MultiplayerProject.Source.Helpers.Factories
{
    public class FireFactory : GameObjectFactory
    {
        public override GameObject GetLaser() => new FireLaser();
        public override GameObject GetExplosion() => new FireExplosion();
    }
}
