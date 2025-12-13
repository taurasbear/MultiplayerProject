using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source.GameObjects.Iterator.Player
{
    public class PlayerIterator : IGameObjectIterator<Source.Player>
    {
        private readonly GameObjectCollection _collection;
        private List<Source.Player> _orderedPlayers;
        private int _currentIndex;

        public PlayerIterator(GameObjectCollection collection)
        {
            _collection = collection;
            _orderedPlayers = _collection.Players
                .OrderBy(p => p.Health)
                .ToList();
            _currentIndex = 0;
        }

        public Source.Player GetNext()
        {
            if (!HasMore())
                return null;

            return _orderedPlayers[_currentIndex++];
        }

        public bool HasMore()
        {
            return _currentIndex < _orderedPlayers.Count;
        }

        public override string ToString()
        {
            return $"PlayerIterator: CurrentIndex={_currentIndex}, TotalPlayers={_orderedPlayers.Count}";
        }
    }
}
