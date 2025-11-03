using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.GameObjects;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Interface for the Player component in the Decorator pattern
    /// Defines all the operations that can be performed on a player
    /// </summary>
    public interface IPlayer
    {
        // Core properties
        bool Active { get; set; }
        int Health { get; set; }
        PlayerColour Colour { get; set; }
        string PlayerName { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        Vector2 Position { get; }
        float Rotation { get; }
        string NetworkID { get; set; }
        int LastSequenceNumberProcessed { get; set; }
        KeyboardMovementInput LastKeyboardMovementInput { get; set; }

        // Core methods
        void Initialize(ContentManager content);
        void Initialize(ContentManager content, PlayerColour colour);
        void Update(GameTime gameTime);
        void Update(float deltaTime);
        void Update(ref Player.ObjectState state, float deltaTime);
        void Draw(SpriteBatch spriteBatch);
        void Draw(SpriteBatch spriteBatch, SpriteFont font);
        
        // Game mechanics
        void SetPlayerState(PlayerUpdatePacket packet);
        void ApplyInputToPlayer(KeyboardMovementInput input, float deltaTime);
        void ApplyInputToPlayer(ref Player.ObjectState state, KeyboardMovementInput input, float deltaTime);
        PlayerUpdatePacket BuildUpdatePacket();
        
        // Enhancement-related methods for decorators
        bool GetHasShield();
        float GetFireRateMultiplier();
        void TakeDamage(int damage);
    }
}