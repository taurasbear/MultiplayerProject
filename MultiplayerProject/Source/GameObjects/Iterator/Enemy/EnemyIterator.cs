using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source.GameObjects.Iterator.Enemy
{
    public class EnemyIterator : IGameObjectIterator<MultiplayerProject.Source.GameObjects.Enemy.Enemy>
    {
        private readonly GameObjectCollection _collection;
        private readonly List<MultiplayerProject.Source.GameObjects.Enemy.Enemy> _orderedEnemies;
        private int _currentIndex;

        public EnemyIterator(GameObjectCollection collection)
        {
            _collection = collection;
            _orderedEnemies = new List<MultiplayerProject.Source.GameObjects.Enemy.Enemy>();

            var parents = _collection.Enemies.Enemies
                .Where(e => e.Active)
                .ToList();

            foreach (var parent in parents)
            {
                _orderedEnemies.Add(parent);
            }

            foreach (var parent in parents)
            {
                foreach (var minion in parent.Minions)
                {
                    if (minion.Active)
                        _orderedEnemies.Add(minion);
                }
            }

            _currentIndex = 0;
        }

        public MultiplayerProject.Source.GameObjects.Enemy.Enemy GetNext()
        {
            if (!HasMore())
                return null;

            return _orderedEnemies[_currentIndex++];
        }

        public bool HasMore()
        {
            return _currentIndex < _orderedEnemies.Count;
        }

        public override string ToString()
        {
            return $"EnemyIterator: CurrentIndex={_currentIndex}, TotalEnemies={_orderedEnemies.Count}";
        }
    }
}
