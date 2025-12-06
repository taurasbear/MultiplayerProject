using MultiplayerProject.Source.GameObjects.Enemy;
using MultiplayerProject.Source;

namespace MultiplayerProject.Source.Visitors
{
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
        public string LogLifetimeReport()
        {
            string result = $"Lasers Fired: {TotalLasersFired}, Explosions: {TotalExplosions}, Enemies Spawned: {TotalEnemiesSpawned}";
            return result;
        }
    }
}