// File: IGameObjectVisitor.cs
// Location: MultiplayerProject/Source/Visitors/IGameObjectVisitor.cs

using MultiplayerProject.Source.GameObjects.Enemy;
using MultiplayerProject.Source.Helpers;

namespace MultiplayerProject.Source.Visitors
{
    /// <summary>
    /// Visitor interface for game object operations.
    /// Defines visit methods for each type of game object.
    /// </summary>
    public interface IGameObjectVisitor
    {
        void Visit(Player player);
        void Visit(Enemy enemy);
        void Visit(Laser laser);
        void Visit(Explosion explosion);
    }
}