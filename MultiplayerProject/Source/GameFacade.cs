using Microsoft.Xna.Framework;
using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.GameObjects.Enemy;
using MultiplayerProject.Source.GameObjects.Iterator;
using MultiplayerProject.Source.Helpers.Factories;
using MultiplayerProject.Source.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerProject.Source
{
    public class GameFacade
    {
        public EnemyManager EnemyManager { get; private set; }
        public LaserManager LaserManager { get; private set; }
        public ExplosionManager ExplosionManager { get; private set; }

        private readonly Dictionary<string, LaserManager> _playerLasers;
        private readonly CollisionManager _collisionManager;

        public GameFacade()
        {
            EnemyManager = new EnemyManager();
            LaserManager = new LaserManager();
            ExplosionManager = new ExplosionManager();

            _playerLasers = new Dictionary<string, LaserManager>();
            _collisionManager = new CollisionManager();
        }

        public void AddPlayer(string playerId)
        {
            if (!_playerLasers.ContainsKey(playerId))
            {
                _playerLasers[playerId] = new LaserManager();
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (var laserManager in _playerLasers.Values)
            {
                laserManager.Update(gameTime);
            }
            EnemyManager.Update(gameTime);
            ExplosionManager.Update(gameTime);
        }

        public Laser FireLaser(string playerId, GameObjectFactory factory, double totalGameTime, float deltaTime, Vector2 position, float rotation, string laserId)
        {
            return _playerLasers[playerId].FireLaserServer(factory, totalGameTime, deltaTime, position, rotation, laserId, playerId);
        }

        public List<CollisionManager.Collision> CheckCollisions(List<Player> players)
        {
            var gameObjectCollection = new GameObjectCollection(players, _playerLasers, EnemyManager);
            return _collisionManager.CheckCollision(gameObjectCollection);
        }

        public void DeactivateLaser(string playerId, string laserId) => _playerLasers[playerId].DeactivateLaser(laserId);

        public void DeactivateEnemy(string enemyId) => EnemyManager.DeactivateEnemy(enemyId);

        public Enemy AddNewEnemy() => EnemyManager.AddEnemy();

        public void SetNextEnemyType(EnemyType enemyType) => EnemyManager.SetEnemyType(enemyType);

        public void NotifyEnemies(EnemyEventType eventType) => EnemyManager.NotifyEnemies(eventType);

        /// <summary>
        /// Apply a visitor to all active lasers across all players
        /// This enables the visitor pattern for server-side statistics tracking
        /// </summary>
        public void ApplyVisitorToAllLasers(IGameObjectVisitor visitor)
        {
            var activeLasers = _playerLasers.Values.SelectMany(lm => lm.Lasers).ToList();
            foreach (var laser in activeLasers)
            {
                laser.Accept(visitor);
            }
        }

        /// <summary>
        /// Apply a visitor to all active explosions
        /// This enables the visitor pattern for server-side statistics tracking
        /// </summary>
        public void ApplyVisitorToAllExplosions(IGameObjectVisitor visitor)
        {
            foreach (var explosion in ExplosionManager.GetEntities())
            {
                explosion.Accept(visitor);
            }
        }

        /// <summary>
        /// Create an explosion at the specified position using the given factory
        /// This is for server-side explosion tracking (no rendering, just statistics)
        /// </summary>
        public Explosion CreateExplosion(Vector2 position, GameObjectFactory factory, Color color)
        {
            ExplosionManager.AddServerExplosion(position, factory, color);
            var explosions = ExplosionManager.GetEntities();
            return explosions.Count > 0 ? explosions[explosions.Count - 1] : null;
        }

        /// <summary>
        /// Get the total count of active lasers across all players
        /// This is for server-side statistics tracking when visitor pattern can't access laser objects directly
        /// </summary>
        public int GetActiveLaserCount()
        {
            return _playerLasers.Values.SelectMany(lm => lm.Lasers).Count(laser => laser.Active);
        }
    }
}
