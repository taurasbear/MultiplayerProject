// File: LifetimeStatisticsVisitor.cs
// Location: MultiplayerProject/Source/Visitors/LifetimeStatisticsVisitor.cs

using MultiplayerProject.Source.GameObjects.Enemy;
using MultiplayerProject.Source;

namespace MultiplayerProject.Source.Visitors
{
    /// <summary>
    /// CONCRETE VISITOR 2: Tracks cumulative statistics during entire game.
    /// Never resets - counts total objects created throughout the game.
    /// </summary>
    public sealed class LifetimeStatisticsVisitor : IGameObjectVisitor
    {
        public int TotalLasersFired { get; private set; }
        public int TotalExplosions { get; private set; }
        public int TotalEnemiesSpawned { get; private set; }

        public void Visit(Player player)
        {
            // Players don't contribute to lifetime stats
        }

        public void Visit(Enemy enemy)
        {
            TotalEnemiesSpawned++;
        }

        public void Visit(Laser laser)
        {
            TotalLasersFired++;
        }

        public void Visit(Explosion explosion)
        {
            TotalExplosions++;
        }
        public void LogLifetimeReport()
        {
            Logger.Instance?.Info($"[V] Total Since Game Start - Lase Fired: {TotalLasersFired}, Expl: {TotalExplosions}, Enem: {TotalEnemiesSpawned}");
        }
    }
}