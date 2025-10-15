using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
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
                case EnemyType.Big:
                    enemyFactory = new BirdEnemyFactory();
                    break;
                case EnemyType.Small:
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
                if (_enemies[i].EnemyAnimation == null)
                    _enemies[i].Update();
                else
                    _enemies[i].Update(gameTime);

                if (_enemies[i].Active == false)
                {
                    _enemies.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                _enemies[i].Draw(spriteBatch);
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

        public void AddEnemy(Vector2 position, string enemyID)
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Create an enemy
            Enemy enemy = _enemyFactory?.CreateEnemy(enemyID) ?? new Enemy(enemyID);

            if (enemy is BirdEnemy)
            {
                enemyAnimation.Initialize(_birdTexture, Vector2.Zero, 0, 68, 68, 7, 30, Color.White, 1f, true);
            }
            else if (enemy is BlackbirdEnemy)
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
        }

        public void DeactivateEnemy(string enemyID)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].EnemyID == enemyID)
                {
                    _enemies[i].Active = false;
                    return;
                }
            }
        }

        public Enemy DeactivateAndReturnEnemy(string enemyID)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].EnemyID == enemyID)
                {
                    _enemies[i].Active = false;
                    return _enemies[i];
                }
            }

            return null;
        }
    }
}
