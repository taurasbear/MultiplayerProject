// File: Enemy.cs
// Location: MultiplayerProject/Source/GameObjects/Enemy/Enemy.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.Visitors;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source.GameObjects.Enemy
{
    public class Enemy : IPrototype<Enemy>, IVisitable
    {
        public Animation EnemyAnimation;
        public Vector2 Position;
        public bool Active;

        public int Health;
        public int Damage;
        public int Value;

        public Vector2 CentrePosition { get { return new Vector2(Position.X - Width / 2, Position.Y - Height / 2); } }
        public int Width { get; set; }
        public int Height { get { return EnemyAnimation?.FrameHeight ?? 0; } }

        public float Scale { get; set; }

        public string EnemyID { get; set; }

        private IMoveAlgorithm _moveAlgorithm;
        private int _maxHealth;

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
            SetMovementAlgorithm(new VerticalPacing());
        }

        public virtual void UpdateOnEnemyEvent(EnemyEventType eventType)
        {
            if (eventType is EnemyEventType.GameCloseToFinishing)
            {
                EnemyAnimation.Scale = 0.5f;
            }
        }

        public void SetMovementAlgorithm(IMoveAlgorithm algorithm)
        {
            _moveAlgorithm = algorithm;
        }

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
            if (_moveAlgorithm != null)
            {
                _moveAlgorithm.Move(ref Position, gameTime);
            }
            else
            {
                Position.X -= Speed;
            }

            if (Health <= 0)
            {
                OnDeath();
                Active = false;
                return;
            }

            if (Position.X < -Width)
            {
                Active = false;
                return;
            }

            EnemyAnimation.Position = Position;
            EnemyAnimation.Update(gameTime);

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
                    Minions.RemoveAt(i);
                }
            }

            UpdateEnemySpecific(gameTime);
        }

        protected virtual void UpdateEnemySpecific(GameTime gameTime)
        {
        }

        protected virtual void OnDeath()
        {
        }

        protected virtual void OnDamage(int damageAmount)
        {
        }

        public void Update()
        {
            if (_moveAlgorithm != null)
            {
                _moveAlgorithm.Move(ref Position, null);
            }
            else
            {
                Position.X -= Speed;
            }

            if (Position.X < -Width || Health <= 0)
            {
                Active = false;
                foreach (var minion in Minions)
                {
                    minion.Active = false;
                }
            }

            for (int i = Minions.Count - 1; i >= 0; i--)
            {
                var minion = Minions[i];
                if (minion.Active)
                {
                    minion.Position = this.Position + new Vector2((this.Width / 2f + minion.Width / 2f) + (minion.Width * i), 0);
                    minion.EnemyAnimation.Position = minion.Position;
                }
                else
                {
                    Minions.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_renderer != null)
            {
                _renderer.Render(spriteBatch, Position, EnemyAnimation);
            }
            else if (EnemyAnimation != null)
            {
                EnemyAnimation.Draw(spriteBatch);
            }

            foreach (var minion in Minions)
            {
                minion.Draw(spriteBatch);
            }
        }

        public Enemy ShallowClone()
        {
            Enemy clone = (Enemy)this.MemberwiseClone();
            clone.EnemyID = Guid.NewGuid().ToString();
            if (this.EnemyAnimation != null)
            {
                clone.EnemyAnimation = this.EnemyAnimation.ShallowClone();
            }
            clone.Minions = new List<Enemy>();
            return clone;
        }

        object IPrototype<Enemy>.DeepClone()
        {
            return DeepClone();
        }

        public Enemy DeepClone()
        {
            Enemy clone = (Enemy)this.MemberwiseClone();
            clone.EnemyID = Guid.NewGuid().ToString();

            if (EnemyAnimation != null)
            {
                clone.EnemyAnimation = (Animation)this.EnemyAnimation.DeepClone();
            }

            clone.Minions = new List<Enemy>();
            foreach (var minion in this.Minions)
            {
                clone.Minions.Add((Enemy)minion.DeepClone());
            }
            return clone;
        }

        public void Accept(IGameObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}