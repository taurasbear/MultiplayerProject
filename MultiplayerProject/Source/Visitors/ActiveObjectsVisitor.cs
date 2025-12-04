// File: ActiveObjectsVisitor.cs
// Location: MultiplayerProject/Source/Visitors/ActiveObjectsVisitor.cs

using MultiplayerProject.Source.GameObjects.Enemy;
using MultiplayerProject.Source.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerProject.Source.Visitors
{
    /// <summary>
    /// CONCRETE VISITOR 1: Counts currently active objects on screen.
    /// Also tracks event totals over the last 5 seconds (resets every 5 seconds).
    /// </summary>
    public sealed class ActiveObjectsVisitor : IGameObjectVisitor
    {
        // Current frame counts
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

        /// <summary>
        /// Manually set laser count for server-side tracking where laser objects can't be properly visited
        /// </summary>
        public void SetActiveLaserCount(int count)
        {
            LaserCount = count;
        }

        /// <summary>
        /// Add event counts to the recent totals (like lifetime stats but resetting every 5 seconds)
        /// Call this when events happen (laser fired, explosion created, etc.)
        /// </summary>
        public void AddRecentEvents(int lasersFired, int explosionsCreated, int enemiesSpawned)
        {
            RecentLaserCount += lasersFired;
            RecentExplosionCount += explosionsCreated;
            RecentEnemyCount += enemiesSpawned;
            // Player count doesn't accumulate - it's just current active players
            RecentPlayerCount = PlayerCount;
        }

        /// <summary>
        /// Call this after visiting all objects AND setting manual counts to finalize the current snapshot
        /// and calculate recent activity totals
        /// </summary>
        public void FinalizeSnapshot()
        {
            // Update recent player count to current active players (always update this)
            RecentPlayerCount = PlayerCount;
        }

        /// <summary>
        /// Check if it's time to log statistics and reset counters
        /// Call this from the game instance when logging
        /// </summary>
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

        /// <summary>
        /// Reset the recent counters after logging
        /// </summary>
        public void ResetRecentCounters()
        {
            RecentEnemyCount = 0;
            RecentLaserCount = 0;
            RecentExplosionCount = 0;
            // Don't reset RecentPlayerCount as it should always show current active players
        }

        public string LogCurrentStatus()
        {
            string result = $"Players: {RecentPlayerCount}, Enemies: {RecentEnemyCount}, Lasers: {RecentLaserCount}, Explosions: {RecentExplosionCount}";
            Logger.Instance?.Info($"[V] In Last 5 Seconds - Play: {RecentPlayerCount}, Enem: {RecentEnemyCount}, Lase: {RecentLaserCount}, Expl: {RecentExplosionCount}");
            return result;
        }

        public void LogRecentActivity()
        {
            // This method is now unused since we combined the logs
        }
    }
}