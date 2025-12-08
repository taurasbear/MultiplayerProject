using Microsoft.Xna.Framework;
using System;

namespace MultiplayerProject.Source.GameObjects.Players.States
{
    /// <summary>
    /// Normal state: Standard gameplay - player can move, shoot, and take damage.
    /// Transitions to DeadState when health reaches 0.
    /// </summary>
    public class NormalState : IPlayerState
    {
        private float _damageCooldown = 0f;
        private const float DAMAGE_COOLDOWN_DURATION = 1.0f; // 1 second between damage instances

        public void Enter(Player player)
        {
            _damageCooldown = 0f; // Reset cooldown when entering state
            Console.WriteLine($"[STATE] Player {player.PlayerName} entered NormalState - Normal gameplay");
        }

        public void Update(Player player, GameTime gameTime)
        {
            // Decrement damage cooldown
            if (_damageCooldown > 0)
            {
                _damageCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // Check if player died
            if (player.Health <= 0)
            {
                Console.WriteLine($"[STATE] Player {player.PlayerName} transitioning to DeadState - Health: {player.Health}");
                player.ChangeState(new DeadState());
            }
        }

        public void HandleInput(Player player, KeyboardMovementInput input, float deltaTime)
        {
            // Normal movement
            player.ApplyInputToPlayerInternal(input, deltaTime);
        }

        public void HandleEnemyCollision(Player player, int damage)
        {
            // Only take damage if cooldown has expired (prevents damage spam)
            if (_damageCooldown <= 0)
            {
                Console.WriteLine($"[DAMAGE] Player {player.PlayerName} BEFORE damage - Health: {player.Health}");
                player.TakeDamage(damage);
                _damageCooldown = DAMAGE_COOLDOWN_DURATION; // Reset cooldown
                Console.WriteLine($"[DAMAGE] Player {player.PlayerName} AFTER taking {damage} damage - Health: {player.Health}");
            }
        }

        public void HandleLaserCollision(Player player, int damage)
        {
            // Take damage from laser
            player.TakeDamage(damage);
            Console.WriteLine($"Player {player.PlayerName} took {damage} damage from laser. Health: {player.Health}");
        }

        public bool CanFire()
        {
            return true;
        }

        public bool IsInvincible()
        {
            return false;
        }

        public float GetAlpha()
        {
            // Fully visible
            return 1.0f;
        }

        public void Exit(Player player)
        {
            Console.WriteLine($"[STATE] Player {player.PlayerName} exiting NormalState");
        }
    }
}
