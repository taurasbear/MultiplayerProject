using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// ShieldDecorator - Adds shield protection that absorbs damage
    /// Extracts the shield logic currently scattered in Player class
    /// </summary>
    public class ShieldDecorator : PlayerDecorator
    {
        private bool shieldActive;
        private readonly int shieldStrength;
        private int currentShieldHealth;
        private float shieldFlashTimer;
        private bool isFlashing;
        private readonly float flashInterval = 0.2f;

        public ShieldDecorator(IPlayer player, int shieldStrength = 1) : base(player)
        {
            this.shieldActive = true;
            this.shieldStrength = shieldStrength;
            this.currentShieldHealth = shieldStrength;
            this.shieldFlashTimer = 0f;
            this.isFlashing = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Update shield visual effects
            if (shieldActive)
            {
                UpdateShieldEffects(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Draw the base player
            base.Draw(spriteBatch, font);
            
            // Draw shield effects if active
            if (shieldActive)
            {
                DrawShieldEffect(spriteBatch);
                DrawShieldIndicator(spriteBatch, font);
            }
        }

        public override bool GetHasShield()
        {
            return shieldActive || base.GetHasShield();
        }

        public override void TakeDamage(int damage)
        {
            // Shield is just visual - pass damage through to base player
            base.TakeDamage(damage);
        }

        private void UpdateShieldEffects(GameTime gameTime)
        {
            // Update flashing effect when shield is low
            if (currentShieldHealth == 1) // Shield is about to break
            {
                shieldFlashTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                if (shieldFlashTimer >= flashInterval)
                {
                    isFlashing = !isFlashing;
                    shieldFlashTimer = 0f;
                }
            }
            else
            {
                isFlashing = false;
            }
        }

        private void DrawShieldEffect(SpriteBatch spriteBatch)
        {
            // Skip drawing if shield is flashing (low health effect)
            if (isFlashing)
                return;
                
            // Draw shield visual effect around the player
            // This would typically be a circular shield texture or particle effect
            Vector2 shieldCenter = Position;
            float shieldRadius = System.Math.Max(Width, Height) / 2 + 10; // Slightly larger than player
            
            // Calculate shield opacity based on health
            float shieldAlpha = (float)currentShieldHealth / shieldStrength;
            Color shieldColor = Color.Cyan * (shieldAlpha * 0.6f); // Semi-transparent
            
            // In a real implementation, you would draw a shield texture or use a shader
            // For now, this is conceptual to show the decorator pattern
            DrawShieldBorder(spriteBatch, shieldCenter, shieldRadius, shieldColor);
        }

        private void DrawShieldBorder(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
        {
            // Conceptual shield border drawing
            // In a real implementation, you would:
            // 1. Use a shield texture/sprite
            // 2. Draw particles around the perimeter
            // 3. Use a shader for the shield effect
            
            // This is just to demonstrate the decorator pattern structure
            int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)(2 * System.Math.PI * i / segments);
                Vector2 point = center + new Vector2(
                    (float)System.Math.Cos(angle) * radius,
                    (float)System.Math.Sin(angle) * radius
                );
                
                // Would draw shield border points/lines here
            }
        }

        private void DrawShieldIndicator(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (font == null) return;
            
            string shieldText = currentShieldHealth > 1 ? 
                $"[SHIELD {currentShieldHealth}]" : 
                "[SHIELD]";
                
            Vector2 textSize = font.MeasureString(shieldText);
            Vector2 textPosition = new Vector2(
                Position.X - textSize.X / 2,
                Position.Y - Height / 2 - textSize.Y - 25 // Above name tag
            );
            
            // Color based on shield health
            Color shieldTextColor = currentShieldHealth > 1 ? Color.Cyan : Color.Yellow;
            
            spriteBatch.DrawString(font, shieldText, textPosition, shieldTextColor);
        }

        private void OnShieldHit()
        {
            // Trigger shield hit effects
            // Could play sound, create particles, etc.
            // AudioManager.Instance?.PlaySound("shieldHit");
        }

        private void OnShieldBreak()
        {
            // Trigger shield break effects
            // Could play sound, create explosion particles, screen shake, etc.
            // AudioManager.Instance?.PlaySound("shieldBreak");
            // ParticleManager.Instance?.CreateShieldBreakEffect(Position);
        }

        /// <summary>
        /// Recharge the shield (for power-ups or score milestones)
        /// </summary>
        public void RechargeShield(int amount = -1)
        {
            if (amount < 0)
                amount = shieldStrength; // Full recharge
                
            currentShieldHealth = System.Math.Min(currentShieldHealth + amount, shieldStrength);
            shieldActive = currentShieldHealth > 0;
            
            Logger.Instance?.Info($"Shield recharged for player {PlayerName}. Health: {currentShieldHealth}/{shieldStrength}");
        }

        /// <summary>
        /// Get the current shield health
        /// </summary>
        public int GetShieldHealth()
        {
            return currentShieldHealth;
        }

        /// <summary>
        /// Get the maximum shield strength
        /// </summary>
        public int GetShieldStrength()
        {
            return shieldStrength;
        }
    }
}