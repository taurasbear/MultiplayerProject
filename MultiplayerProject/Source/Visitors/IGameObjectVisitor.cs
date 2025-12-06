using MultiplayerProject.Source.GameObjects.Enemy;
using MultiplayerProject.Source.Helpers;

namespace MultiplayerProject.Source.Visitors
{
    public interface IGameObjectVisitor
    {
        void Visit(Player player);
        void Visit(Enemy enemy);
        void Visit(Laser laser);
        void Visit(Explosion explosion);
    }
}