using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using MultiplayerProject.Source.GameObjects.Enemy;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    public class EnemyManager
    {
        public List<Enemy> Enemies { get { return _enemies; } }
        public float Width { get { return _width; } }

        private Texture2D _mineTexture;

        private Texture2D _birdTexture;

        private Texture2D _blackbirdTexture;
        private List<Enemy> _enemies;

        private Random _random;

        private float _width;
        
        // Time-based movement changes
        private double _totalGameTime;
        private double _lastMovementChangeTime;
        private const double SECONDS_PER_MOVEMENT_CHANGE = 5.0;
        private int _currentMovementIndex;
        
        // Bridge Pattern: Current renderer for all enemies
        private IEnemyRenderer _currentRenderer;

        private EnemyFactory _enemyFactory;

        // Add ExplosionManager
        private ExplosionManager _explosionManager;
        public ExplosionManager ExplosionManager => _explosionManager;

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

            // Initialize ExplosionManager
            _explosionManager = new ExplosionManager();
        }

        public void SetEnemyType(EnemyType enemyType)
        {
            EnemyFactory enemyFactory = null;

            switch (enemyType)
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
            
            // Initialize Bridge Pattern with AnimatedRenderer (original look)
            InitializeRenderer(content, "animated");
        }
        
        /// <summary>
        /// Initialize or change the renderer used for all enemies
        /// </summary>
        public void InitializeRenderer(ContentManager content, string rendererType = "animated")
        {
            switch (rendererType.ToLower())
            {
                case "animated":
                    _currentRenderer = new AnimatedRenderer();
                    break;
                case "static":
                    _currentRenderer = new StaticRenderer();
                    break;
                case "particle":
                    _currentRenderer = new ParticleRenderer();
                    break;
                default:
                    _currentRenderer = new AnimatedRenderer(); // Default fallback
                    break;
            }
            
            _currentRenderer.Initialize(content);
            
            // Update existing enemies to use new renderer
            foreach (var enemy in _enemies)
            {
                enemy.SetRenderer(_currentRenderer);
            }
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
                var enemy = _enemies[i];

                enemy.Update(gameTime);

                // Update minions (Prototype pattern - deep clones)
                for (int j = enemy.Minions.Count - 1; j >= 0; j--)
                {
                    var minion = enemy.Minions[j];
                    minion.Update(gameTime);

                    // Remove inactive minions
                    if (!minion.Active)
                    {
                        enemy.Minions.RemoveAt(j);
                    }
                }

                // Check if the enemy (parent or clone) is inactive and should be removed
                if (enemy.Active == false)
                {
                    _enemies.RemoveAt(i);
                }
            }
        }

        public void NotifyEnemies(EnemyEventType eventType)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                enemy.UpdateOnEnemyEvent(eventType);

                // Notify minions too
                foreach (var minion in enemy.Minions)
                {
                    minion.UpdateOnEnemyEvent(eventType);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                enemy?.Draw(spriteBatch);

                // Draw minions (Prototype pattern - deep clones)
                foreach (var minion in enemy.Minions)
                {
                    minion?.Draw(spriteBatch);
                }
            }
        }

        public Enemy AddEnemy()
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(Application.WINDOW_WIDTH + _width / 2,
                _random.Next(100, Application.WINDOW_HEIGHT - 100));

            // Create appropriate enemy type using Bridge pattern
            Enemy enemy = _enemyFactory?.CreateEnemy() ?? new Enemy();
            enemy.SetRenderer(_currentRenderer);

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

            // Set the current movement pattern
            SetCurrentMovementPattern(enemy);

            // Add the enemy to the active enemies list
            _enemies.Add(enemy);

            return enemy;
        }

        public Enemy AddEnemy(EnemyType type, Vector2 position, string enemyType = "standard")
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Create an enemy using Bridge Pattern with current renderer
            Enemy enemy;
            switch (type)
            {
                case EnemyType.Bird:
                    enemy = new BirdEnemy(_currentRenderer);
                    if (_birdTexture != null)
                    {
                        enemyAnimation.Initialize(_birdTexture, Vector2.Zero, 0, 68, 68, 7, 30, Color.White, 1f, true);
                    }
                    else
                    {
                        Logger.Instance?.Warning("BirdTexture is null! Using fallback.");
                    }
                    break;
                case EnemyType.Blackbird:
                    enemy = new BlackbirdEnemy(_currentRenderer);
                    if (_blackbirdTexture != null)
                    {
                        enemyAnimation.Initialize(_blackbirdTexture, Vector2.Zero, 0, 16, 18, 8, 30, Color.White, 1f, true);
                    }
                    else
                    {
                        Logger.Instance?.Warning("BlackbirdTexture is null! Using fallback.");
                    }
                    break;
                case EnemyType.Mine:
                default:
                    enemy = new Enemy();
                    enemy.SetRenderer(_currentRenderer);
                    if (_mineTexture != null)
                    {
                        enemyAnimation.Initialize(_mineTexture, Vector2.Zero, 0, 47, 61, 8, 30, Color.White, 1f, true);
                    }
                    else
                    {
                        Logger.Instance?.Warning("MineTexture is null for Mine type! Using fallback.");
                    }
                    break;
            }

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            // Set the current movement pattern
            SetCurrentMovementPattern(enemy);

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

                // Also change movement for minions (they should move with parent)
                foreach (var minion in enemy.Minions)
                {
                    IMoveAlgorithm minionMovement = (IMoveAlgorithm)Activator.CreateInstance(currentMovementType);
                    minion.SetMovementAlgorithm(minionMovement);
                }
            }
            
            // Move to next movement in cycle
            _currentMovementIndex = (_currentMovementIndex + 1) % movementTypes.Length;
        }
        
        /// <summary>
        /// Change the renderer for all enemies at runtime
        /// </summary>
        public void ChangeRenderer(ContentManager content, string rendererType)
        {
            // Create new renderer
            IEnemyRenderer newRenderer;
            switch (rendererType.ToLower())
            {
                case "animated":
                    newRenderer = new AnimatedRenderer();
                    break;
                case "static":
                    newRenderer = new StaticRenderer();
                    break;
                case "particle":
                    newRenderer = new ParticleRenderer();
                    break;
                default:
                    return; // Invalid renderer type
            }
            
            newRenderer.Initialize(content);
            
            // Update all existing enemies with new renderer
            foreach (var enemy in _enemies)
            {
                enemy.SetRenderer(newRenderer);
            }
            
            _currentRenderer = newRenderer;
        }
    }
}
