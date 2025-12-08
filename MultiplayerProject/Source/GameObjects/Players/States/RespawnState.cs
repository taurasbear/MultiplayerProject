using Microsoft.Xna.Framework;
using System;

namespace MultiplayerProject.Source.GameObjects.Players.States
{
    /// <summary>
    /// Respawn state: Player has just respawned.
    /// Player is flashing and invincible for 3 seconds, but can move and shoot.
    /// After 3 seconds, transitions to NormalState.
    /// </summary>
    public class RespawnState : IPlayerState
    {
        private float _elapsedTime;
        private const float INVINCIBILITY_DURATION = 3.0f;
        private const float FLASH_FREQUENCY = 0.2f; // Flash every 0.2 seconds

        public void Enter(Player player)
        {
            _elapsedTime = 0f;
            
            // Reset player stats for respawn
            player.Health = Application.PLAYER_STARTING_HEALTH;
            
            // Reset position to top-left corner (spawn point)
            Vector2 spawnPosition = new Vector2(100, 100);
            player.SetPosition(spawnPosition);
            
            Console.WriteLine($"[STATE] Player {player.PlayerName} entered RespawnState - Invincible for {INVINCIBILITY_DURATION} seconds");
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
            // Player can move normally in respawn state
            player.ApplyInputToPlayerInternal(input, deltaTime);
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
            // Can shoot in respawn state
            return true;
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
            Console.WriteLine($"[STATE] Player {player.PlayerName} exiting RespawnState - Now vulnerable");
        }
    }
}
