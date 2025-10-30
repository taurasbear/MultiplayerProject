using Microsoft.Xna.Framework;
using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.GameObjects.Enemy;
using MultiplayerProject.Source.Helpers.Factories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerProject.Source
{
    public class GameFacade
    {
        private readonly Dictionary<string, LaserManager> _playerLasers;
        private readonly CollisionManager _collisionManager;
        private readonly EnemyManager _enemyManager;

        public GameFacade()
        {
            _playerLasers = new Dictionary<string, LaserManager>();
            _collisionManager = new CollisionManager();
            _enemyManager = new EnemyManager();
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
            _enemyManager.Update(gameTime);
        }

        public Laser FireLaser(string playerId, GameObjectFactory factory, double totalGameTime, float deltaTime, Vector2 position, float rotation, string laserId)
        {
            return _playerLasers[playerId].FireLaserServer(factory, totalGameTime, deltaTime, position, rotation, laserId, playerId);
        }

        public List<CollisionManager.Collision> CheckCollisions(List<Player> players)
        {
            var activeLasers = _playerLasers.Values.SelectMany(lm => lm.Lasers).ToList();
            return _collisionManager.CheckCollision(players, _enemyManager.Enemies, activeLasers);
        }

        public void DeactivateLaser(string playerId, string laserId) => _playerLasers[playerId].DeactivateLaser(laserId);

        public void DeactivateEnemy(string enemyId) => _enemyManager.DeactivateEnemy(enemyId);

        public Enemy AddNewEnemy() => _enemyManager.AddEnemy();

        public void SetNextEnemyType(EnemyType enemyType) => _enemyManager.SetEnemyType(enemyType);
    }
}
