using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using MultiplayerProject.Source.GameObjects.Enemy;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    class EnemyManager
    {
        public List<Enemy> Enemies { get { return _enemies; } }

        private Texture2D _mineTexture;

        private Texture2D _birdTexture;

        private Texture2D _blackbirdTexture;

        private List<Enemy> _enemies;

        private Random _random;

        private float _width;

        private EnemyFactory _enemyFactory;

        public EnemyManager()
        {
            // Initialize the enemies list
            _enemies = new List<Enemy>();

            // Initialize our random number generator
            _random = new Random();

            _width = 47;
        }

        public void SetEnemyType(EnemyType enemyType)
        {
            EnemyFactory enemyFactory = null;

            switch(enemyType)
            {
                case EnemyType.Bird:
                    enemyFactory = new BirdEnemyFactory();
                    break;
                case EnemyType.Blackbird:
                    enemyFactory = new BlackbirdEnemyFactory();
                    break;
            }

            _enemyFactory = enemyFactory;
        }

        public void Initalise(ContentManager content)
        {
            _mineTexture = content.Load<Texture2D>("mineAnimation");
            _birdTexture = content.Load<Texture2D>("birdAnimation");
            _blackbirdTexture = content.Load<Texture2D>("blackbirdAnimation");
        }

        public void Update(GameTime gameTime)
        {
            // Update the Enemies
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var enemy = _enemies[i];

                enemy.Update(gameTime);

                // Check if the enemy (parent or clone) is inactive and should be removed
                if (enemy.Active == false)
                {
                    _enemies.RemoveAt(i);
                }
            }
        }

        public void NotifyEnemies(EnemyEventType eventType)
        {
            if(eventType is EnemyEventType.GameCloseToFinishing)
            {
                Console.WriteLine("--> We got enemy event for game finishing up!");
                for (int i = 0; i < _enemies.Count; i++)
                {
                    var enemy = _enemies[i];
                    if (enemy is BlackbirdEnemy)
                    {
                        enemy.Speed += 8f;
                    }
                    else if (!(enemy is BirdEnemy))
                    {
                        enemy.EnemyAnimation.Scale = 0.5f;
                    }
                }
            }
            else if(eventType is EnemyEventType.PlayerShot)
            {
                for (int i = 0; i < _enemies.Count; i++)
                {
                    var enemy = _enemies[i];
                    if (enemy is BlackbirdEnemy)
                    {
                        enemy.Position.Y -= 10f;
                    }
                    else if (enemy is BirdEnemy)
                    {
                        enemy.Position.Y += 10f;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                enemy?.Draw(spriteBatch);
            }
        }

        public Enemy AddEnemy()
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(Application.WINDOW_WIDTH + _width / 2,
                _random.Next(100, Application.WINDOW_HEIGHT - 100));

            // Create an enemy
            Enemy enemy = _enemyFactory?.CreateEnemy() ?? new Enemy();

            if(enemy is BirdEnemy)
            {
                enemyAnimation.Initialize(_birdTexture, Vector2.Zero, 0, 68, 68, 7, 30, Color.White, 1f, true);
            }
            else if(enemy is BlackbirdEnemy)
            {
                enemyAnimation.Initialize(_blackbirdTexture, Vector2.Zero, 0, 16, 18, 8, 30, Color.White, 1f, true);
            }
            else
            {
                enemyAnimation.Initialize(_mineTexture, Vector2.Zero, 0, 47, 61, 8, 30, Color.White, 1f, true);
            }

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            // Add the enemy to the active enemies list
            _enemies.Add(enemy);

            return enemy;
        }

        public Enemy AddEnemy(EnemyType type, Vector2 position)
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Create an enemy
            Enemy enemy;
            switch (type)
            {
                case EnemyType.Bird:
                    enemy = new BirdEnemy();
                    enemyAnimation.Initialize(_birdTexture, Vector2.Zero, 0, 68, 68, 7, 30, Color.White, 1f, true);
                    break;
                case EnemyType.Blackbird:
                    enemy = new BlackbirdEnemy();
                    enemyAnimation.Initialize(_blackbirdTexture, Vector2.Zero, 0, 16, 18, 8, 30, Color.White, 1f, true);
                    break;
                default:
                    enemy = new Enemy();
                    enemyAnimation.Initialize(_mineTexture, Vector2.Zero, 0, 47, 61, 8, 30, Color.White, 1f, true);
                    break;
            }

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            _enemies.Add(enemy);
            return enemy;
        }

        public void DeactivateEnemy(string enemyID)
        {
            // First, check top-level enemies
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].EnemyID == enemyID)
                {
                    _enemies[i].Active = false;
                    return;
                }

                // If not found, check the minions of the current top-level enemy
                foreach (var minion in _enemies[i].Minions)
                {
                    if (minion.EnemyID == enemyID)
                    {
                        minion.Active = false;
                        return;
                    }
                }
            }
        }

        public Enemy DeactivateAndReturnEnemy(string enemyID)
        {
            // First, check top-level enemies
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].EnemyID == enemyID)
                {
                    _enemies[i].Active = false;
                    return _enemies[i];
                }

                // If not found, check the minions of the current top-level enemy
                foreach (var minion in _enemies[i].Minions)
                {
                    if (minion.EnemyID == enemyID)
                    {
                        minion.Active = false;
                        return minion;
                    }
                }
            }

            return null;
        }
    }
}
