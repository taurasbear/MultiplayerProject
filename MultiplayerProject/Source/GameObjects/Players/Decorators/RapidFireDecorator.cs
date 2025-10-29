using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// RapidFireDecorator - Increases the player's firing rate based on score or temporary power-ups
    /// Extracts the rapid fire logic currently in LaserManager.UpdateFireRate()
    /// </summary>
    public class RapidFireDecorator : PlayerDecorator
    {
        private readonly float fireRateMultiplier;
        private readonly bool isTemporary;
        private float timeRemaining;
        private readonly float maxMultiplier;

        /// <summary>
        /// Create a rapid fire decorator with a fixed multiplier
        /// </summary>
        public RapidFireDecorator(IPlayer player, float multiplier, float duration = 0f, float maxMultiplier = 3.0f) 
            : base(player)
        {
            this.fireRateMultiplier = multiplier;
            this.isTemporary = duration > 0f;
            this.timeRemaining = duration;
            this.maxMultiplier = maxMultiplier;
        }

        /// <summary>
        /// Create a score-based rapid fire decorator (mimics current LaserManager logic)
        /// </summary>
        public static RapidFireDecorator FromScore(IPlayer player, int playerScore, float maxMultiplier = 3.0f)
        {
            // Replicate the logic from LaserManager.UpdateFireRate()
            // Base rate increases by 25% for every 3 points
            float scoreMultiplier = 1.0f + (playerScore / 3) * 0.25f;
            scoreMultiplier = System.Math.Min(scoreMultiplier, maxMultiplier);
            
            return new RapidFireDecorator(player, scoreMultiplier, 0f, maxMultiplier);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Handle temporary rapid fire expiration
            if (isTemporary && timeRemaining > 0)
            {
                timeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                if (timeRemaining <= 0)
                {
                    // Could trigger event or callback when rapid fire expires
                    Logger.Instance?.Info($"Rapid fire expired for player {PlayerName}");
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Draw the base player
            base.Draw(spriteBatch, font);
            
            // Draw rapid fire indicator
            if (IsRapidFireActive() && font != null)
            {
                DrawRapidFireIndicator(spriteBatch, font);
            }
        }

        public override float GetFireRateMultiplier()
        {
            if (!IsRapidFireActive())
            {
                return base.GetFireRateMultiplier();
            }
            
            // Multiply the base fire rate by our multiplier
            return base.GetFireRateMultiplier() * fireRateMultiplier;
        }

        private bool IsRapidFireActive()
        {
            return !isTemporary || timeRemaining > 0;
        }

        private void DrawRapidFireIndicator(SpriteBatch spriteBatch, SpriteFont font)
        {
            string indicator;
            Color indicatorColor;
            
            if (isTemporary)
            {
                indicator = $"RAPID FIRE {fireRateMultiplier:F1}x ({timeRemaining:F1}s)";
                // Change color based on time remaining
                if (timeRemaining < 3f)
                    indicatorColor = Color.Red;
                else if (timeRemaining < 5f)
                    indicatorColor = Color.Yellow;
                else
                    indicatorColor = Color.Green;
            }
            else
            {
                indicator = $"RAPID FIRE {fireRateMultiplier:F1}x";
                indicatorColor = Color.Orange;
            }
            
            Vector2 textSize = font.MeasureString(indicator);
            Vector2 indicatorPosition = new Vector2(
                Position.X - textSize.X / 2,
                Position.Y + Height / 2 + 10 // Below the player
            );
            
            spriteBatch.DrawString(font, indicator, indicatorPosition, indicatorColor);
        }

        /// <summary>
        /// Get the actual fire rate in shots per minute (for LaserManager integration)
        /// </summary>
        public float GetActualFireRate(float baseFireRate = 200f)
        {
            if (!IsRapidFireActive())
            {
                return baseFireRate;
            }
            
            float enhancedRate = baseFireRate * GetFireRateMultiplier();
            return System.Math.Min(enhancedRate, baseFireRate * maxMultiplier);
        }

        /// <summary>
        /// Check if the rapid fire effect is about to expire
        /// </summary>
        public bool IsAboutToExpire(float warningTime = 3f)
        {
            return isTemporary && timeRemaining > 0 && timeRemaining <= warningTime;
        }
    }
}