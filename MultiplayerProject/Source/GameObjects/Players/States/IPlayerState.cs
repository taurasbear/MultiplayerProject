using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source.GameObjects.Players.States
{
    /// <summary>
    /// State pattern interface for Player states.
    /// Defines behavior for different player states (Idle, Normal, Dead, Respawn).
    /// </summary>
    public interface IPlayerState
    {
        /// <summary>
        /// Called when entering this state
        /// </summary>
        void Enter(Player player);

        /// <summary>
        /// Update state logic (timers, transitions, etc.)
        /// </summary>
        void Update(Player player, GameTime gameTime);

        /// <summary>
        /// Handle player input in this state
        /// </summary>
        void HandleInput(Player player, KeyboardMovementInput input, float deltaTime);

        /// <summary>
        /// Handle collision with enemy in this state
        /// </summary>
        void HandleEnemyCollision(Player player, int damage);

        /// <summary>
        /// Handle collision with laser in this state
        /// </summary>
        void HandleLaserCollision(Player player, int damage);

        /// <summary>
        /// Check if player can fire in this state
        /// </summary>
        bool CanFire();

        /// <summary>
        /// Check if player is invincible in this state
        /// </summary>
        bool IsInvincible();

        /// <summary>
        /// Get alpha value for rendering (for flashing effect)
        /// </summary>
        float GetAlpha();

        /// <summary>
        /// Called when exiting this state
        /// </summary>
        void Exit(Player player);
    }
}
