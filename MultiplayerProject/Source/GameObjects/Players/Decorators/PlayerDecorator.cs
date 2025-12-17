using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.GameObjects;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Abstract base class for all player decorators
    /// Implements the Decorator pattern by forwarding all calls to the wrapped player
    /// </summary>
    public abstract class PlayerDecorator : IPlayer
    {
        protected IPlayer wrappedPlayer;

        /// <summary>
        /// Public access to the wrapped player for unwrapping decorators
        /// </summary>
        public IPlayer WrappedPlayer => wrappedPlayer;

        public PlayerDecorator(IPlayer player)
        {
            wrappedPlayer = player;
        }

        // Forward all properties to the wrapped player
        public virtual bool Active 
        { 
            get => wrappedPlayer.Active; 
            set => wrappedPlayer.Active = value; 
        }
        
        public virtual int Health 
        { 
            get => wrappedPlayer.Health; 
            set => wrappedPlayer.Health = value; 
        }
        
        public virtual PlayerColour Colour 
        { 
            get => wrappedPlayer.Colour; 
            set => wrappedPlayer.Colour = value; 
        }

        public virtual int Score
        {
            get => wrappedPlayer.Score;
            set => wrappedPlayer.Score = value;
        }

        public virtual float Speed
        {
            get => wrappedPlayer.Speed;
            set => wrappedPlayer.Speed = value;
        }

        public virtual float FireRate
        {
            get => wrappedPlayer.FireRate;
            set => wrappedPlayer.FireRate = value;
        }


        public virtual string PlayerName 
        { 
            get => wrappedPlayer.PlayerName; 
            set => wrappedPlayer.PlayerName = value; 
        }
        
        public virtual int Width 
        { 
            get => wrappedPlayer.Width; 
            set => wrappedPlayer.Width = value; 
        }
        
        public virtual int Height 
        { 
            get => wrappedPlayer.Height; 
            set => wrappedPlayer.Height = value; 
        }
        
        public virtual Vector2 Position => wrappedPlayer.Position;
        public virtual float Rotation => wrappedPlayer.Rotation;
        
        public virtual string NetworkID 
        { 
            get => wrappedPlayer.NetworkID; 
            set => wrappedPlayer.NetworkID = value; 
        }
        
        public virtual int LastSequenceNumberProcessed 
        { 
            get => wrappedPlayer.LastSequenceNumberProcessed; 
            set => wrappedPlayer.LastSequenceNumberProcessed = value; 
        }
        
        public virtual KeyboardMovementInput LastKeyboardMovementInput 
        { 
            get => wrappedPlayer.LastKeyboardMovementInput; 
            set => wrappedPlayer.LastKeyboardMovementInput = value; 
        }

        // Forward all methods to the wrapped player
        public virtual void Initialize(ContentManager content)
        {
            wrappedPlayer.Initialize(content);
        }

        public virtual void Initialize(ContentManager content, PlayerColour colour)
        {
            wrappedPlayer.Initialize(content, colour);
        }

        public virtual void Update(GameTime gameTime)
        {
            wrappedPlayer.Update(gameTime);
        }

        public virtual void Update(float deltaTime)
        {
            wrappedPlayer.Update(deltaTime);
        }

        public virtual void Update(ref Player.ObjectState state, float deltaTime)
        {
            wrappedPlayer.Update(ref state, deltaTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            wrappedPlayer.Draw(spriteBatch);
        }

        public virtual void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            wrappedPlayer.Draw(spriteBatch, font);
        }

        public virtual void SetPlayerState(PlayerUpdatePacket packet)
        {
            wrappedPlayer.SetPlayerState(packet);
        }

        public virtual void ApplyInputToPlayer(KeyboardMovementInput input, float deltaTime)
        {
            wrappedPlayer.ApplyInputToPlayer(input, deltaTime);
        }

        public virtual void ApplyInputToPlayer(ref Player.ObjectState state, KeyboardMovementInput input, float deltaTime)
        {
            wrappedPlayer.ApplyInputToPlayer(ref state, input, deltaTime);
        }

        public virtual PlayerUpdatePacket BuildUpdatePacket()
        {
            return wrappedPlayer.BuildUpdatePacket();
        }

        // Forward enhancement methods - can be overridden by concrete decorators
        public virtual bool GetHasShield()
        {
            return wrappedPlayer.GetHasShield();
        }

        public virtual float GetFireRateMultiplier()
        {
            return wrappedPlayer.GetFireRateMultiplier();
        }

        public virtual void TakeDamage(int damage)
        {
            wrappedPlayer.TakeDamage(damage);
        }
    }
}