using MultiplayerProject.Source.GameObjects.Iterator.Enemy;
using MultiplayerProject.Source.GameObjects.Iterator.Laser;
using MultiplayerProject.Source.GameObjects.Iterator.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source.GameObjects.Iterator
{
    public class GameObjectCollection
    {
        public List<Source.Player> Players { get; set; }

        public Dictionary<string, LaserManager> Lasers { get; set; }
        
        public EnemyManager Enemies { get; set; }

        public GameObjectCollection(List<Source.Player> players, Dictionary<string, LaserManager> lasers, EnemyManager enemies)
        {
            Players = players;
            Lasers = lasers;
            Enemies = enemies;
        }

        public PlayerIterator CreatePlayerIterator()
        {
            return new PlayerIterator(this);
        }

        public LaserIterator CreateLaserIterator()
        {
            return new LaserIterator(this);
        }

        public EnemyIterator CreateEnemyIterator()
        {
            return new EnemyIterator(this);
        }
    }
}
