using Microsoft.Xna.Framework;
using MultiplayerProject.Source.GameObjects.Enemy;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// BirdEnemy - represents a basic enemy with standard behavior
    /// Extends Enemy class with Bridge pattern support
    /// Includes all StandardEnemy logic
    /// </summary>
    class BirdEnemy : Enemy
    {
        public BirdEnemy() : base()
        {
            Width = 94;
            Scale = 3f;
            // Standard enemy initialization
        }

        public BirdEnemy(string ID) : base(ID)
        {
            Width = 94;
            Scale = 3f;
            // Standard enemy with specific ID
        }
        
        public BirdEnemy(IEnemyRenderer renderer) : base()
        {
            Width = 94;
            Scale = 3f;
            // Bridge pattern constructor
            SetRenderer(renderer);
        }
        
        public BirdEnemy(string ID, IEnemyRenderer renderer) : base(ID)
        {
            Width = 94;
            Scale = 3f;
            // Bridge pattern constructor with ID
            SetRenderer(renderer);
        }
        
        public override void Update(GameTime gameTime)
        {
            // Call base Enemy update logic - preserves existing behavior
            base.Update(gameTime);
            
            // Add any bird enemy specific behavior here if needed
        }

        protected override void UpdateEnemySpecific(GameTime gameTime)
        {
            // Bird enemy has no special update behavior
            // Just basic movement handled by base class
        }

        protected override void OnDeath()
        {
            // Bird enemy death behavior
            // Could add death effects, sounds, etc. here
        }

        protected override void OnDamage(int damageAmount)
        {
            // Bird enemy damage behavior
            // Could add damage effects, sounds, etc. here
            base.OnDamage(damageAmount);
        }

        public override void UpdateOnEnemyEvent(EnemyEventType eventType)
        {
            if (eventType is EnemyEventType.PlayerShot)
            {
                Position.Y += 25f;
            }
        }
    }
}