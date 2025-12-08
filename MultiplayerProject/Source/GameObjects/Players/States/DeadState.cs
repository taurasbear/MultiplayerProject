using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MultiplayerProject.Source.GameObjects.Players.States
{
    /// <summary>
    /// Dead state: Player cannot move or shoot.
    /// After 5 seconds, can press respawn key (R) to respawn.
    /// Respawning resets score to 0 and transitions to RespawnState.
    /// </summary>
    public class DeadState : IPlayerState
    {
        private float _elapsedTime;
        private const float RESPAWN_COOLDOWN = 5.0f;
        private bool _canRespawn;

        public void Enter(Player player)
        {
            _elapsedTime = 0f;
            _canRespawn = false;
            player.Active = false; // Mark as inactive
            Console.WriteLine($"[STATE] Player {player.PlayerName} entered DeadState - Cannot respawn for {RESPAWN_COOLDOWN} seconds");
        }

        public void Update(Player player, GameTime gameTime)
        {
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Enable respawn after cooldown
            if (!_canRespawn && _elapsedTime >= RESPAWN_COOLDOWN)
            {
                _canRespawn = true;
                Console.WriteLine($"[STATE] Player {player.PlayerName} can now respawn - Press R to respawn");
            }
        }

        public void HandleInput(Player player, KeyboardMovementInput input, float deltaTime)
        {
            // Cannot move when dead
            // Check for respawn key (R key)
            // Note: This needs to be checked from a higher level with actual KeyboardState
            // The KeyboardMovementInput only has directional inputs
        }

        /// <summary>
        /// Check if respawn key is pressed. Should be called from Player class with full keyboard state.
        /// </summary>
        public bool CheckRespawnInput(Keys respawnKey, KeyboardState currentKeyboard, KeyboardState previousKeyboard)
        {
            if (_canRespawn && currentKeyboard.IsKeyDown(respawnKey) && previousKeyboard.IsKeyUp(respawnKey))
            {
                return true;
            }
            return false;
        }

        public void HandleEnemyCollision(Player player, int damage)
        {
            // Already dead - no effect
        }

        public void HandleLaserCollision(Player player, int damage)
        {
            // Already dead - no effect
        }

        public bool CanFire()
        {
            return false;
        }

        public bool IsInvincible()
        {
            // Technically invincible since dead
            return true;
        }

        public float GetAlpha()
        {
            // Semi-transparent to show dead state
            return 0.3f;
        }

        public void Exit(Player player)
        {
            player.Active = true; // Reactivate on exit
            Console.WriteLine($"[STATE] Player {player.PlayerName} exiting DeadState - Respawning");
        }
    }
}
