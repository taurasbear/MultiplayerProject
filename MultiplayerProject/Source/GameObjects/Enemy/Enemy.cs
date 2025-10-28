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
        public int Height { get { return EnemyAnimation.FrameHeight; } }

        public float Scale { get; set; }

        public string EnemyID { get; set; }

        const float ENEMY_MOVE_SPEED = 6f;

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

            Damage = ENEMY_DAMAGE;

            Value = ENEMY_DEATH_SCORE_INCREASE;
        }

        public void Update(GameTime gameTime)
        {
            // The enemy always moves to the left so decrement its x position
            Position.X -= ENEMY_MOVE_SPEED;

            // Update the position of the Animation
            EnemyAnimation.Position = Position;

            // Update Animation
            EnemyAnimation.Update(gameTime);

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
                    minion.EnemyAnimation.Update(gameTime);
                }
                else
                {
                    Minions.RemoveAt(i); // Remove inactive minion
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (EnemyAnimation != null)
            {
                // Draw the animation
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
