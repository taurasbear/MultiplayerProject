using MultiplayerProject.Source.GameObjects.Enemy;
using MultiplayerProject.Source.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerProject.Source.Visitors
{
    public sealed class ActiveObjectsVisitor : IGameObjectVisitor
    {
        public int PlayerCount { get; private set; }
        public int EnemyCount { get; private set; }
        public int LaserCount { get; private set; }
        public int ExplosionCount { get; private set; }

        // Recent activity tracking (last 5 seconds) - event totals that reset every 5 seconds
        private DateTime _windowStartTime;
        private DateTime _lastLogTime;
        private const double WINDOW_SIZE_SECONDS = 5.0;
        
        public int RecentPlayerCount { get; private set; }
        public int RecentEnemyCount { get; private set; }
        public int RecentLaserCount { get; private set; }
        public int RecentExplosionCount { get; private set; }

        public ActiveObjectsVisitor()
        {
            _windowStartTime = DateTime.UtcNow;
            _lastLogTime = DateTime.UtcNow;
        }

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

        public void SetActiveLaserCount(int count)
        {
            LaserCount = count;
        }

        public void AddRecentEvents(int lasersFired, int explosionsCreated, int enemiesSpawned)
        {
            RecentLaserCount += lasersFired;
            RecentExplosionCount += explosionsCreated;
            RecentEnemyCount += enemiesSpawned;
            RecentPlayerCount = PlayerCount;
        }

        public void FinalizeSnapshot()
        {
            RecentPlayerCount = PlayerCount;
        }

        public bool ShouldLogAndReset()
        {
            var currentTime = DateTime.UtcNow;
            var timeSinceLastLog = (currentTime - _lastLogTime).TotalSeconds;
            
            if (timeSinceLastLog >= WINDOW_SIZE_SECONDS)
            {
                _lastLogTime = currentTime;
                return true;
            }
            return false;
        }

        public void ResetRecentCounters()
        {
            RecentEnemyCount = 0;
            RecentLaserCount = 0;
            RecentExplosionCount = 0;
        }

        public string LogCurrentStatus()
        {
            string result = $"Players: {RecentPlayerCount}, Enemies: {RecentEnemyCount}, Lasers: {RecentLaserCount}, Explosions: {RecentExplosionCount}";
            return result;
        }

    }
}