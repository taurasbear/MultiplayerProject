// File: ActiveObjectsVisitor.cs
// Location: MultiplayerProject/Source/Visitors/ActiveObjectsVisitor.cs

using MultiplayerProject.Source.GameObjects.Enemy;
using MultiplayerProject.Source.Helpers;

namespace MultiplayerProject.Source.Visitors
{
    /// <summary>
    /// CONCRETE VISITOR 1: Counts currently active objects on screen.
    /// Resets every frame to show current game state.
    /// </summary>
    public sealed class ActiveObjectsVisitor : IGameObjectVisitor
    {
        public int PlayerCount { get; private set; }
        public int EnemyCount { get; private set; }
        public int LaserCount { get; private set; }
        public int ExplosionCount { get; private set; }

        public void Reset()
        {
            PlayerCount = 0;
            EnemyCount = 0;
            LaserCount = 0;
            ExplosionCount = 0;
        }

        public void Visit(Player player)
        {
            PlayerCount++;
        }

        public void Visit(Enemy enemy)
        {
            EnemyCount++;
            EnemyCount += enemy.Minions.Count;
        }

        public void Visit(Laser laser)
        {
            LaserCount++;
        }

        public void Visit(Explosion explosion)
        {
            ExplosionCount++;
        }

        public void LogCurrentStatus()
        {
            Logger.Instance?.Info($"[V] Active Now - Play: {PlayerCount}, Enem: {EnemyCount}, Lase: {LaserCount}, Expl: {ExplosionCount}");
        }
    }
}