using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source.GameObjects.Iterator.Laser
{
    public class LaserIterator : IGameObjectIterator<Source.Laser>
    {
        private readonly GameObjectCollection _collection;
        private readonly List<Source.Laser> _cachedLasers = new List<Source.Laser>();
        private int _currentIndex;

        public LaserIterator(GameObjectCollection collection)
        {
            _collection = collection;
            _currentIndex = 0;
        }

        public Source.Laser GetNext()
        {
            if (!HasMore())
                return null;

            return _cachedLasers[_currentIndex++];
        }

        public bool HasMore()
        {
            if (_cachedLasers.Count == 0)
            {
                CacheClosestLasersToEnemies();
            }

            return _currentIndex < _cachedLasers.Count;
        }

        private void CacheClosestLasersToEnemies()
        {
            var allLasers = new List<Source.Laser>();
            if (_collection != null && _collection.Lasers != null)
            {
                foreach (var manager in _collection.Lasers.Values)
                {
                    if (manager?.Lasers != null)
                        allLasers.AddRange(manager.Lasers.Where(l => l != null && l.Active));
                }
            }

            var enemies = _collection?.Enemies?.Enemies ?? new List<Source.GameObjects.Enemy.Enemy>();

            _cachedLasers.AddRange(
                allLasers
                .OrderBy(laser =>
                    enemies.Count == 0
                        ? float.MaxValue
                        : enemies.Min(enemy =>
                            Vector2.Distance(laser.Position, enemy.Position)
                        )
                )
            );
        }

        public override string ToString()
        {
            return $"LaserIterator: CurrentIndex={_currentIndex}, TotalLasers={_cachedLasers.Count}";
        }
    }
}
