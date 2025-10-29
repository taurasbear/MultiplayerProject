using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// StandardEnemy - represents a basic enemy with standard behavior
    /// Extends Enemy class with Bridge pattern support
    /// </summary>
    public class StandardEnemy : Enemy
    {
        public StandardEnemy() : base()
        {
            // Standard enemy initialization
        }
        
        public StandardEnemy(string ID) : base(ID)
        {
            // Standard enemy with specific ID
        }
        
        public StandardEnemy(IEnemyRenderer renderer) : base()
        {
            // Bridge pattern constructor
            SetRenderer(renderer);
        }
        
        public StandardEnemy(string ID, IEnemyRenderer renderer) : base(ID)
        {
            // Bridge pattern constructor with ID
            SetRenderer(renderer);
        }
        
        public override void Update(GameTime gameTime)
        {
            // Call base Enemy update logic - preserves existing behavior
            base.Update(gameTime);
            
            // Add any standard enemy specific behavior here if needed
        }

        protected override void UpdateEnemySpecific(GameTime gameTime)
        {
            // Standard enemy has no special update behavior
            // Just basic movement handled by base class
        }

        protected override void OnDeath()
        {
            // Standard enemy death behavior
            // Could add death effects, sounds, etc. here
        }

        protected override void OnDamage(int damageAmount)
        {
            // Standard enemy damage behavior
            // Could add damage effects, sounds, etc. here
            base.OnDamage(damageAmount);
        }
    }
}