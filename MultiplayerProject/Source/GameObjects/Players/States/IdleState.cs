using Microsoft.Xna.Framework;
using System;

namespace MultiplayerProject.Source.GameObjects.Players.States
{
    /// <summary>
    /// Idle state: Player is invincible and flashing for 3 seconds at match start.
    /// After 3 seconds, transitions to NormalState.
    /// </summary>
    public class IdleState : IPlayerState
    {
        private float _elapsedTime;
        private const float INVINCIBILITY_DURATION = 3.0f;
        private const float FLASH_FREQUENCY = 0.2f; // Flash every 0.2 seconds

        public void Enter(Player player)
        {
            _elapsedTime = 0f;
            
            // Set initial spawn position at top-left corner
            Vector2 spawnPosition = new Vector2(100, 100);
            player.SetPosition(spawnPosition);
            
            Console.WriteLine($"[STATE] Player {player.PlayerName} entered IdleState - Invincible for {INVINCIBILITY_DURATION} seconds");
        }

        public void Update(Player player, GameTime gameTime)
        {
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Transition to NormalState after 3 seconds
            if (_elapsedTime >= INVINCIBILITY_DURATION)
            {
                player.ChangeState(new NormalState());
            }
        }

        public void HandleInput(Player player, KeyboardMovementInput input, float deltaTime)
        {
            // Cannot move in idle state - frozen at match start
            // Do nothing with input
        }

        public void HandleEnemyCollision(Player player, int damage)
        {
            // Invincible - no damage taken
        }

        public void HandleLaserCollision(Player player, int damage)
        {
            // Invincible - no damage taken
        }

        public bool CanFire()
        {
            // Cannot shoot in idle state
            return false;
        }

        public bool IsInvincible()
        {
            return true;
        }

        public float GetAlpha()
        {
            // Flashing effect - alternates between visible and semi-transparent
            float flashCycle = _elapsedTime % FLASH_FREQUENCY;
            return (flashCycle < FLASH_FREQUENCY / 2) ? 1.0f : 0.3f;
        }

        public void Exit(Player player)
        {
            Console.WriteLine($"[STATE] Player {player.PlayerName} exiting IdleState");
        }
    }
}
