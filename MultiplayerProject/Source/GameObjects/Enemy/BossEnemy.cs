using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// BossEnemy - represents a powerful enemy with enhanced behavior
    /// Extends Enemy class with additional boss-specific features
    /// </summary>
    public class BossEnemy : Enemy
    {
        private bool _berserkMode = false;
        private int _originalDamage;
        
        public BossEnemy() : base()
        {
            InitializeBoss();
        }
        
        public BossEnemy(string ID) : base(ID)
        {
            InitializeBoss();
        }
        
        public BossEnemy(IEnemyRenderer renderer) : base()
        {
            SetRenderer(renderer);
            InitializeBoss();
        }
        
        public BossEnemy(string ID, IEnemyRenderer renderer) : base(ID)
        {
            SetRenderer(renderer);
            InitializeBoss();
        }
        
        private void InitializeBoss()
        {
            // Boss has more health and damage
            Health = 100; // Override default health
            Damage = 30;  // Override default damage
            Value = 500;  // Higher score value
            
            _originalDamage = Damage;
        }
        
        public override void Update(GameTime gameTime)
        {
            // Call base Enemy update logic
            base.Update(gameTime);
            
            // Boss-specific behavior
            CheckBerserkMode();
        }

        protected override void UpdateEnemySpecific(GameTime gameTime)
        {
            // Boss-specific update logic can go here
            // For example: special attack patterns, phase changes, etc.
        }

        protected override void OnDeath()
        {
            // Boss death behavior - could trigger special effects, cutscenes, etc.
            Logger.Instance?.Info($"Boss enemy {EnemyID} has been defeated!");
            
            // Could switch to particle renderer for death effect
            // SetRenderer(new ParticleRenderer());
        }

        protected override void OnDamage(int damageAmount)
        {
            // Boss damage behavior - could trigger defensive measures
            Logger.Instance?.Info($"Boss enemy {EnemyID} took {damageAmount} damage!");
            
            // Could change renderer based on damage state
            if (Health <= 30 && _berserkMode)
            {
                // Switch to particle renderer in berserk mode for visual effect
                // SetRenderer(new ParticleRenderer());
            }
            
            base.OnDamage(damageAmount);
        }
        
        private void CheckBerserkMode()
        {
            // Enter berserk mode when health is low
            if (Health <= 30 && !_berserkMode)
            {
                EnterBerserkMode();
            }
        }
        
        private void EnterBerserkMode()
        {
            _berserkMode = true;
            Damage = _originalDamage * 2; // Double damage in berserk mode
            
            // Could add more aggressive movement or special attacks here
            Logger.Instance?.Info($"Boss enemy {EnemyID} entered berserk mode!");
        }
    }
}