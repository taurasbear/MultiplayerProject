using Google.Protobuf.WellKnownTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source.GameObjects.Enemy
{
    public class Enemy : IPrototype<Enemy>
    {
        public Animation EnemyAnimation;
        public Vector2 Position;
        public bool Active;

        public int Health;
        public int Damage; // The amount of damage the enemy inflicts on the player ship
        public int Value;  // The amount of score the enemy will give to the player

        public Vector2 CentrePosition { get { return new Vector2(Position.X - Width / 2, Position.Y - Height / 2); } }
        public int Width { get; set; }
        public int Height { get { return EnemyAnimation?.FrameHeight ?? 0; } }

        public float Scale { get; set; }

        public string EnemyID { get; set; }

        private IMoveAlgorithm _moveAlgorithm;
        private int _maxHealth;
        
        // Bridge Pattern: Reference to renderer implementation
        private IEnemyRenderer _renderer;

        public float Speed { get; set; } = 4f;

        const int ENEMY_STARTING_HEALTH = 10;
        const int ENEMY_DAMAGE = 10;
        const int ENEMY_DEATH_SCORE_INCREASE = 100;

        public List<Enemy> Minions { get; set; }

        public Enemy()
        {
            EnemyID = Guid.NewGuid().ToString();
            Width = 47;
            Scale = 1f;
            Minions = new List<Enemy>();
        }

        public Enemy(string ID)
        {
            EnemyID = ID;
            Scale = 1f;
            Minions = new List<Enemy>();
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

        public virtual void UpdateOnEnemyEvent(EnemyEventType eventType)
        {
            if(eventType is EnemyEventType.GameCloseToFinishing)
            {
                EnemyAnimation.Scale = 0.5f;
            }
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
                Position.X -= Speed;
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

            // Update and clean up inactive minions
            for (int i = Minions.Count - 1; i >= 0; i--)
            {
                var minion = Minions[i];
                if (minion.Active)
                {
                    minion.Position = this.Position + new Vector2((this.Width / 2f + minion.Width / 2f) + (minion.Width * i), 0);
                    minion.EnemyAnimation.Position = minion.Position;
                    minion.EnemyAnimation.Update(gameTime);
                }
                else
                {
                    Minions.RemoveAt(i); // Remove inactive minion
                }
            }

            // Call enemy-specific update logic
            UpdateEnemySpecific(gameTime);
        }

        // Abstract method that must be implemented by concrete enemy classes
        protected virtual void UpdateEnemySpecific(GameTime gameTime)
        {
            // Default implementation does nothing
        }

        // Abstract method for enemy-specific death behavior
        protected virtual void OnDeath()
        {
            // Default death behavior - can be overridden
        }

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
                Position.X -= Speed;
            }

            // If the enemy is past the screen or its health reaches 0 then deactivate it
            if (Position.X < -Width || Health <= 0)
            {
                // By setting the Active flag to false, the game will remove this objet from the
                // active game list
                Active = false;
                // Also deactivate all minions
                foreach (var minion in Minions)
                {
                    minion.Active = false;
                }
            }

            // Update and clean up inactive minions
            for (int i = Minions.Count - 1; i >= 0; i--)
            {
                var minion = Minions[i];
                if (minion.Active)
                {
                    minion.Position = this.Position + new Vector2((this.Width / 2f + minion.Width / 2f) + (minion.Width * i), 0);
                    minion.EnemyAnimation.Position = minion.Position;
                    // Note: This parameterless Update() method doesn't have gameTime, so we can't update animation here
                    // Animation updates should be done in the Update(GameTime gameTime) overload
                }
                else
                {
                    Minions.RemoveAt(i); // Remove inactive minion
                }
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

            // Draw minions after the parent so they appear on top.
            // This is crucial for deep copies to show their minions.
            foreach (var minion in Minions)
            {
                minion.Draw(spriteBatch); // Recursively draw minions
            }
        }

        public Enemy ShallowClone()
        {
            // Shallow copy: MemberwiseClone copies value types and references as-is
            Enemy clone = (Enemy)this.MemberwiseClone();
            clone.EnemyID = Guid.NewGuid().ToString(); // Give the clone a new ID
            // For demonstration, give the shallow clone an empty minion list
            if (this.EnemyAnimation != null)
            {
                // Even in a shallow clone, we need a separate animation object to avoid state conflicts.
                clone.EnemyAnimation = this.EnemyAnimation.ShallowClone();
            }
            clone.Minions = new List<Enemy>(); // Ensure shallow clone has no minions
            return clone;
        }

        object IPrototype<Enemy>.DeepClone()
        {
            return DeepClone();
        }

        public Enemy DeepClone()
        {
            // Deep copy: Clone value types and create new instances for referenced objects
            Enemy clone = (Enemy)this.MemberwiseClone();
            clone.EnemyID = Guid.NewGuid().ToString(); // Give the clone a new ID

            if (EnemyAnimation != null)
            {
                clone.EnemyAnimation = (Animation)this.EnemyAnimation.DeepClone();
            }
            // Deep copy of minions
            clone.Minions = new List<Enemy>();
            foreach (var minion in this.Minions)
            {
                // Recursively deep clone minions
                clone.Minions.Add((Enemy)minion.DeepClone());
            }
            return clone;
        }
    }
}
