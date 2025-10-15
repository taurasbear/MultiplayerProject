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
        public float Width { get { return _width; } }

        private Texture2D _enemyTexture;

        private List<Enemy> _enemies;

        private Random _random;
        private float _width;
        
        // Time-based movement changes
        private double _totalGameTime;
        private double _lastMovementChangeTime;
        private const double SECONDS_PER_MOVEMENT_CHANGE = 5.0;
        private int _currentMovementIndex;

        public EnemyManager()
        {
            // Initialize the enemies list
            _enemies = new List<Enemy>();

            // Initialize our random number generator
            _random = new Random();

            _width = 47;
            
            // Initialize time-based movement change tracking
            _totalGameTime = 0.0;
            _lastMovementChangeTime = 0.0;
            _currentMovementIndex = 0;
        }

        public void Initalise(ContentManager content)
        {
            _enemyTexture = content.Load<Texture2D>("mineAnimation");
        }

        public void Update(GameTime gameTime)
        {
            // Track total game time
            _totalGameTime += gameTime.ElapsedGameTime.TotalSeconds;
            
            // Check if we should change enemy movements based on time
            if (_totalGameTime - _lastMovementChangeTime >= SECONDS_PER_MOVEMENT_CHANGE)
            {
                _lastMovementChangeTime = _totalGameTime;
                ChangeAllEnemyMovements();
            }
            
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

            // Initialize the animation with the correct animation information
            enemyAnimation.Initialize(_enemyTexture, Vector2.Zero, 0, 47, 61, 8, 30, Color.White, 1f, true);

            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(Application.WINDOW_WIDTH + _width / 2,
                _random.Next(100, Application.WINDOW_HEIGHT - 100));

            // Create an enemy
            Enemy enemy = new Enemy();

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            // Set the current movement pattern
            SetCurrentMovementPattern(enemy);

            // Add the enemy to the active enemies list
            _enemies.Add(enemy);

            return enemy;
        }

        public void AddEnemy(Vector2 position, string enemyID)
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Initialize the animation with the correct animation information
            enemyAnimation.Initialize(_enemyTexture, Vector2.Zero, 0, 47, 61, 8, 30, Color.White, 1f, true);

            // Create an enemy
            Enemy enemy = new Enemy(enemyID);

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            // Set the current movement pattern
            SetCurrentMovementPattern(enemy);

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

        public void DamageEnemy(string enemyID, int damage)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].EnemyID == enemyID)
                {
                    _enemies[i].TakeDamage(damage);
                    return;
                }
            }
        }

        public void DamageAllEnemies(int damage)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                _enemies[i].TakeDamage(damage);
            }
        }

        public void OnEnemyKilled()
        {
            // This method can be kept for other purposes if needed,
            // but movement changes are now handled by time-based system in Update()
        }
        
        private void SetCurrentMovementPattern(Enemy enemy)
        {
            // Create ordered list of movement algorithms (same as in ChangeAllEnemyMovements)
            var movementTypes = new Type[] 
            {
                typeof(VerticalPacing),  // 0
                typeof(ZigZag),          // 1  
                typeof(Spinning),        // 2
                typeof(FlyRight)         // 3
            };
            
            // Get the current movement type (use previous index since ChangeAllEnemyMovements increments it)
            int currentIndex = _currentMovementIndex == 0 ? movementTypes.Length - 1 : _currentMovementIndex - 1;
            Type currentMovementType = movementTypes[currentIndex];
            
            // Create and set the movement algorithm
            IMoveAlgorithm movement = (IMoveAlgorithm)Activator.CreateInstance(currentMovementType);
            enemy.SetMovementAlgorithm(movement);
        }
        
        private void ChangeAllEnemyMovements()
        {
            if (_enemies.Count == 0) return;
            
            // Create ordered list of movement algorithms (pacing -> zigzag -> spinning -> right)
            var movementTypes = new Type[] 
            {
                typeof(VerticalPacing),  // 0
                typeof(ZigZag),          // 1  
                typeof(Spinning),        // 2
                typeof(FlyRight)         // 3
            };
            
            // Get the current movement type based on cycle
            Type currentMovementType = movementTypes[_currentMovementIndex];
            
            // Change movement for ALL enemies to the same pattern
            for (int i = 0; i < _enemies.Count; i++)
            {
                Enemy enemy = _enemies[i];
                
                // Create and set the new movement algorithm
                IMoveAlgorithm newMovement = (IMoveAlgorithm)Activator.CreateInstance(currentMovementType);
                enemy.SetMovementAlgorithm(newMovement);
            }
            
            // Move to next movement in cycle
            _currentMovementIndex = (_currentMovementIndex + 1) % movementTypes.Length;
        }
    }
}
