using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Example demonstrating how to use the Player Decorator pattern
    /// Shows how existing game logic can be replaced with decorators
    /// </summary>
    public class PlayerDecoratorUsageExample
    {
        /// <summary>
        /// Example: Replacing current shield logic in GameScene.UpdatePlayerShields()
        /// </summary>
        public IPlayer ApplyShieldBasedOnScore(IPlayer basePlayer, int playerScore)
        {
            // Current logic: player.HasShield = playerScore >= SHIELD_SCORE_THRESHOLD;
            // New decorator approach:
            
            const int SHIELD_SCORE_THRESHOLD = 5;
            
            if (playerScore >= SHIELD_SCORE_THRESHOLD)
            {
                // Only add shield if player doesn't already have one
                if (!basePlayer.GetHasShield())
                {
                    return new ShieldDecorator(basePlayer);
                }
            }
            
            return basePlayer;
        }

        /// <summary>
        /// Example: Replacing current rapid fire logic in GameScene.Update()
        /// </summary>
        public IPlayer ApplyRapidFireBasedOnScore(IPlayer basePlayer, int playerScore)
        {
            // Current logic: _laserManager.UpdateFireRate(currentScore);
            // New decorator approach:
            
            if (playerScore > 0)
            {
                return RapidFireDecorator.FromScore(basePlayer, playerScore);
            }
            
            return basePlayer;
        }

        /// <summary>
        /// Example: Creating a fully enhanced player with multiple decorators
        /// </summary>
        public IPlayer CreateEnhancedPlayer(Player basePlayer, int playerScore, bool hasSpeedPowerup = false)
        {
            IPlayer enhancedPlayer = basePlayer;

            // Add name tag enhancement (always applied)
            enhancedPlayer = new NameTagDecorator(enhancedPlayer, showEnhancements: true);

            // Add shield based on score
            if (playerScore >= 5)
            {
                enhancedPlayer = new ShieldDecorator(enhancedPlayer);
            }

            // Add rapid fire based on score
            if (playerScore > 0)
            {
                enhancedPlayer = RapidFireDecorator.FromScore(enhancedPlayer, playerScore);
            }

            // Add temporary speed boost (hypothetical - not implemented)
            if (hasSpeedPowerup)
            {
                // enhancedPlayer = new SpeedBoostDecorator(enhancedPlayer, 1.5f, 10f);
            }

            return enhancedPlayer;
        }

        /// <summary>
        /// Example: How to integrate with existing GameScene code
        /// </summary>
        public void GameSceneIntegrationExample()
        {
            /*
            // In GameScene.cs, replace this logic:
            
            private void UpdatePlayerShields()
            {
                const int SHIELD_SCORE_THRESHOLD = 5;
                
                foreach (var player in _players.Values)
                {
                    int playerScore = _GUI.GetPlayerScore(player.NetworkID);
                    player.HasShield = playerScore >= SHIELD_SCORE_THRESHOLD;
                }
            }
            
            // With this decorator-based approach:
            
            private void UpdatePlayerEnhancements()
            {
                foreach (var kvp in _players.ToList()) // ToList to avoid modification during iteration
                {
                    var player = kvp.Value;
                    int playerScore = _GUI.GetPlayerScore(player.NetworkID);
                    
                    // Apply enhancements using decorators
                    IPlayer enhancedPlayer = CreateEnhancedPlayer((Player)player, playerScore);
                    
                    // Replace the player in the dictionary if it was enhanced
                    if (enhancedPlayer != player)
                    {
                        _players[kvp.Key] = enhancedPlayer;
                    }
                }
            }
            
            // And replace this logic:
            
            // Update fire rate based on score
            _laserManager.UpdateFireRate(currentScore);
            
            // With:
            
            // Fire rate is now handled by RapidFireDecorator automatically
            // LaserManager can query player.GetFireRateMultiplier() instead
            */
        }

        /// <summary>
        /// Example: Stacking multiple decorators for power-ups
        /// </summary>
        public IPlayer ApplyTemporaryPowerUp(IPlayer basePlayer, string powerUpType)
        {
            switch (powerUpType.ToLower())
            {
                case "shield":
                    return new ShieldDecorator(basePlayer);
                    
                case "rapidfire":
                    return new RapidFireDecorator(basePlayer, 2.0f, 15f); // 2x fire rate for 15 seconds
                    
                case "super":
                    // Stack multiple enhancements
                    return new RapidFireDecorator(
                        new ShieldDecorator(basePlayer),
                        2.5f, 20f
                    );
                    
                default:
                    return basePlayer;
            }
        }

        /// <summary>
        /// Example: Dynamic enhancement removal (when effects expire)
        /// </summary>
        public IPlayer RemoveExpiredEnhancements(IPlayer decoratedPlayer)
        {
            // This is more complex as you need to unwrap decorators
            // In practice, you might use a PlayerEnhancementManager to track this
            
            // For temporary effects, the decorators themselves handle expiration
            // (see RapidFireDecorator.Update() method)
            
            return decoratedPlayer;
        }
    }
}