using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    public abstract class Enemy
    {
        public Animation EnemyAnimation;
        public Vector2 Position;
        public bool Active;

        public int Health;
        public int Damage; // The amount of damage the enemy inflicts on the player ship
        public int Value;  // The amount of score the enemy will give to the player

        public Vector2 CentrePosition { get { return new Vector2(Position.X - Width/2, Position.Y - Height / 2); } }
        public int Width { get; set; }
        public int Height { get { return EnemyAnimation?.FrameHeight ?? 0; } }

        public string EnemyID { get; set; }

        private IMoveAlgorithm _moveAlgorithm;
        private int _maxHealth;
        
        // Bridge Pattern: Reference to renderer implementation
        private IEnemyRenderer _renderer;

        const float ENEMY_MOVE_SPEED = 4f;

        const int ENEMY_STARTING_HEALTH = 10;
        const int ENEMY_DAMAGE = 10;
        const int ENEMY_DEATH_SCORE_INCREASE = 100;

        public Enemy()
        {
            EnemyID = Guid.NewGuid().ToString();
            Width = 47;
        }

        public Enemy(string ID)
        {
            EnemyID = ID;
        }

        public void Initialize(Animation animation, Vector2 position)
        {    
            EnemyAnimation = animation;

            Position = position;

            Active = true;

            Health = ENEMY_STARTING_HEALTH;
            _maxHealth = ENEMY_STARTING_HEALTH;

            Damage = ENEMY_DAMAGE;

            Value = ENEMY_DEATH_SCORE_INCREASE;

            // Set initial movement strategy
            SetMovementAlgorithm(new VerticalPacing());
        }

        public void SetMovementAlgorithm(IMoveAlgorithm algorithm)
        {
            _moveAlgorithm = algorithm;
        }
        
        // Bridge Pattern: Set the renderer implementation
        public void SetRenderer(IEnemyRenderer renderer)
        {
            _renderer = renderer;
        }

        public void MoveDirection(string direction)
        {
            _moveAlgorithm?.BehaveDifferently();
        }

        public void TakeDamage(int damageAmount)
        {
            Health -= damageAmount;
            OnDamage(damageAmount);
        }

        public virtual void Update(GameTime gameTime)
        {
            // Use strategy pattern for movement with GameTime
            if (_moveAlgorithm != null)
            {
                _moveAlgorithm.Move(ref Position, gameTime);
            }
            else
            {
                // Fallback to default movement if no strategy is set
                Position.X -= ENEMY_MOVE_SPEED;
            }

            // Check for death before enemy-specific update
            if (Health <= 0)
            {
                OnDeath();
                Active = false;
                return;
            }

            // If the enemy is past the screen then deactivate it
            if (Position.X < -Width)
            {
                Active = false;
                return;
            }

            // Update the position of the Animation
            EnemyAnimation.Position = Position;

            // Update Animation
            EnemyAnimation.Update(gameTime);

            // Call enemy-specific update logic
            UpdateEnemySpecific(gameTime);
        }

        // Abstract method that must be implemented by concrete enemy classes
        protected abstract void UpdateEnemySpecific(GameTime gameTime);

        // Abstract method for enemy-specific death behavior
        protected abstract void OnDeath();

        // Virtual method for taking damage - can be overridden for special behavior
        protected virtual void OnDamage(int damageAmount)
        {
            // Default damage behavior - can be overridden
        }

        public void Update()
        {
            // Use strategy pattern for movement
            if (_moveAlgorithm != null)
            {
                _moveAlgorithm.Move(ref Position, null);
            }
            else
            {
                // Fallback to default movement if no strategy is set
                Position.X -= ENEMY_MOVE_SPEED;
            }

            // If the enemy is past the screen or its health reaches 0 then deactivate it
            if (Position.X < -Width || Health <= 0)
            {
                // By setting the Active flag to false, the game will remove this objet from the
                // active game list
                Active = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Bridge Pattern: Use renderer if available, otherwise fallback to original
            if (_renderer != null)
            {
                _renderer.Render(spriteBatch, Position, EnemyAnimation);
            }
            else if (EnemyAnimation != null)
            {
                // Fallback to original rendering for backward compatibility
                EnemyAnimation.Draw(spriteBatch);
            }
        }
    }
}
